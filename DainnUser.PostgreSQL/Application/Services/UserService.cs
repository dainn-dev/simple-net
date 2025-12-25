using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Application.Events;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Domain.Entities;
using InfraExceptions = DainnUser.PostgreSQL.Infrastructure.Exceptions;
using DainnUser.PostgreSQL.Infrastructure.Persistence;
using DainnUser.PostgreSQL.Infrastructure.Telemetry;
using DainnUser.PostgreSQL.Infrastructure.Metrics;
using DainnUser.PostgreSQL.Options;

namespace DainnUser.PostgreSQL.Application.Services;

/// <summary>
/// Service for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IEmailService _emailService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly UserManagementOptions _options;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IAuthService authService,
        IAuditService auditService,
        IEventPublisher eventPublisher,
        IEmailService emailService,
        IRoleService roleService,
        IPermissionService permissionService,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<ChangePasswordDto> changePasswordValidator,
        UserManagementOptions options,
        ILogger<UserService> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authService = authService;
        _auditService = auditService;
        _eventPublisher = eventPublisher;
        _emailService = emailService;
        _roleService = roleService;
        _permissionService = permissionService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _options = options;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration data.</param>
    /// <returns>The registered user profile.</returns>
    public virtual async Task<UserProfileDto> RegisterAsync(RegisterDto dto)
    {
        using var activity = Telemetry.Source.StartActivity("User.Register");
        activity?.SetTag("user.email", dto.Email);
        
        // Validate DTO
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");
            throw new InfraExceptions.ValidationException(validationResult.ToDictionary());
        }

        // Pre-register hook
        await OnBeforeRegister(dto);

        // Check for duplicate email
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new InfraExceptions.BusinessRuleException("An account with this email already exists.");
        }

        // Create new user
        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new InfraExceptions.ValidationException(errors);
        }

        // Publish event
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName
        });

        // Audit log
        var ipAddress = GetClientIpAddress();
        await _auditService.LogAsync("Register", user.Id, $"User registered: {user.Email}", ipAddress);

        // Increment registration metric
        AuthMetrics.RegistrationCounter.Add(1);

        // Structured logging
        _logger.LogInformation("User {UserId} registered with email {Email} from {IP}", 
            user.Id, user.Email, ipAddress);

        activity?.SetTag("user.id", user.Id.ToString());
        activity?.SetStatus(ActivityStatusCode.Ok);

        // Map to DTO
        return await MapToProfileDtoAsync(user);
    }

    /// <summary>
    /// Authenticates a user and performs login.
    /// </summary>
    /// <param name="dto">The login credentials.</param>
    /// <returns>The authentication token response.</returns>
    public virtual async Task<TokenResponseDto> LoginAsync(LoginDto dto)
    {
        using var activity = Telemetry.Source.StartActivity("User.Login");
        activity?.SetTag("user.email", dto.Email);
        
        // Validate DTO
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");
            throw new InfraExceptions.ValidationException(validationResult.ToDictionary());
        }

        // Find user
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            throw new InfraExceptions.BusinessRuleException("Invalid email or password.");
        }

        // Check if account is locked
        if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
        {
            throw new InfraExceptions.BusinessRuleException("This account is temporarily locked. Please try again later.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            throw new InfraExceptions.BusinessRuleException("This account has been deactivated.");
        }

        // Try password
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        
        if (!signInResult.Succeeded)
        {
            // Increment failed attempts
            user.FailedLoginAttempts++;
            
            // Check if we should lock the account
            if (user.FailedLoginAttempts >= _options.SecuritySettings.MaxFailedLoginAttempts)
            {
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(_options.SecuritySettings.LockoutDurationMinutes);
                user.FailedLoginAttempts = 0;
                
                await _eventPublisher.PublishAsync(new UserLockedEvent
                {
                    UserId = user.Id,
                    Until = user.LockoutUntil.Value
                });
            }
            
            await _userManager.UpdateAsync(user);
            var failedLoginIpAddress = GetClientIpAddress();
            await _auditService.LogAsync("LoginFailed", user.Id, "Invalid password attempt", failedLoginIpAddress);
            
            // Increment failed login metric
            AuthMetrics.LoginCounter.Add(1, new KeyValuePair<string, object?>("result", "failed"));
            
            // Structured logging
            _logger.LogWarning("Failed login attempt for user {UserId} from {IP}", 
                user.Id, failedLoginIpAddress);
            
            activity?.SetStatus(ActivityStatusCode.Error, "Invalid credentials");
            throw new InfraExceptions.BusinessRuleException("Invalid email or password.");
        }

        // Successful login
        // Increment successful login metric
        AuthMetrics.LoginCounter.Add(1, new KeyValuePair<string, object?>("result", "success"));
        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Post-login hook
        await OnAfterLogin(user);

        // Publish event
        await _eventPublisher.PublishAsync(new UserLoggedInEvent
        {
            UserId = user.Id,
            IpAddress = "Unknown"
        });

        // Audit log
        var ipAddress = GetClientIpAddress();
        await _auditService.LogAsync("Login", user.Id, $"Successful login", ipAddress);

        // Structured logging
        _logger.LogInformation("User {UserId} logged in from {IP}", user.Id, ipAddress);

        // Generate tokens
        var accessToken = await _authService.GenerateJwtAsync(user);
        var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

        activity?.SetTag("user.id", user.Id.ToString());
        activity?.SetStatus(ActivityStatusCode.Ok);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _options.JwtSettings.AccessTokenHours * 3600
        };
    }

    /// <summary>
    /// Refreshes an authentication token using a refresh token.
    /// </summary>
    /// <param name="dto">The refresh token data.</param>
    /// <returns>The new authentication token response.</returns>
    public virtual async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        using var activity = Telemetry.Source.StartActivity("User.RefreshToken");
        var user = await _authService.ValidateRefreshTokenAsync(dto.RefreshToken);
        if (user == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Invalid refresh token");
            throw new InfraExceptions.BusinessRuleException("Invalid refresh token.");
        }

        activity?.SetTag("user.id", user.Id.ToString());

        // Generate new tokens
        var accessToken = await _authService.GenerateJwtAsync(user);
        var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

        // Revoke old refresh token
        var oldRefreshToken = await _userManager.Users
            .SelectMany(u => u.RefreshTokens.Where(rt => rt.Token == dto.RefreshToken))
            .FirstOrDefaultAsync();

        if (oldRefreshToken != null)
        {
            oldRefreshToken.IsRevoked = true;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = _options.JwtSettings.AccessTokenHours * 3600
        };
    }

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The profile update data.</param>
    /// <returns>The updated user profile.</returns>
    public virtual async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User", userId);
        }

        user.FullName = dto.FullName;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        // Publish event
        await _eventPublisher.PublishAsync(new ProfileUpdatedEvent
        {
            UserId = user.Id,
            OldEmail = null
        });

        // Audit log
        await _auditService.LogAsync("UpdateProfile", userId, "Profile updated", "Unknown");

        return await MapToProfileDtoAsync(user);
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="dto">The password change data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        // Validate DTO
        var validationResult = await _changePasswordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new InfraExceptions.ValidationException(validationResult.ToDictionary());
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User", userId);
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new InfraExceptions.ValidationException(errors);
        }

        // Revoke all refresh tokens
        await _authService.RevokeAllTokensAsync(userId);

        // Publish event
        await _eventPublisher.PublishAsync(new PasswordChangedEvent { UserId = user.Id });

        // Audit log
        await _auditService.LogAsync("ChangePassword", userId, "Password changed", "Unknown");
    }

    /// <summary>
    /// Locks a user account.
    /// </summary>
    /// <param name="dto">The lock user data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task LockUserAsync(LockUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User", dto.UserId);
        }

        user.LockoutUntil = dto.LockUntil ?? DateTime.UtcNow.AddMinutes(_options.SecuritySettings.LockoutDurationMinutes);
        await _userManager.UpdateAsync(user);

        // Revoke all refresh tokens
        await _authService.RevokeAllTokensAsync(user.Id);

        // Publish event
        await _eventPublisher.PublishAsync(new UserLockedEvent
        {
            UserId = user.Id,
            Until = user.LockoutUntil.Value
        });

        // Audit log
        await _auditService.LogAsync("LockUser", user.Id, $"User locked until {user.LockoutUntil}", "Unknown");
    }

    /// <summary>
    /// Gets a user's profile information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user profile.</returns>
    public virtual async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User", userId);
        }

        return await MapToProfileDtoAsync(user);
    }

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <param name="dto">The email confirmation data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ConfirmEmailAsync(ConfirmEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User", dto.UserId);
        }

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            throw new InfraExceptions.BusinessRuleException($"Failed to confirm email: {string.Join(", ", errors)}");
        }

        user.EmailConfirmedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _auditService.LogAsync("ConfirmEmail", user.Id, "Email confirmed", "Unknown");
    }

    /// <summary>
    /// Initiates a password reset flow.
    /// </summary>
    /// <param name="dto">The forgot password data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetAsync(dto.Email, token);
        }

        // Don't reveal if user exists or not for security
        await _auditService.LogAsync("ForgotPassword", user?.Id, $"Password reset requested for {dto.Email}", "Unknown");
    }

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="dto">The password reset data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            throw new InfraExceptions.NotFoundException("User with email", dto.Email);
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new InfraExceptions.ValidationException(errors);
        }

        // Revoke all refresh tokens
        await _authService.RevokeAllTokensAsync(user.Id);

        // Publish event
        await _eventPublisher.PublishAsync(new PasswordChangedEvent { UserId = user.Id });

        // Audit log
        await _auditService.LogAsync("ResetPassword", user.Id, "Password reset via token", "Unknown");
    }

    /// <summary>
    /// Exports all users to a list for backup or migration purposes.
    /// </summary>
    /// <returns>A list of exported user data.</returns>
    public virtual async Task<List<ExportUserDto>> ExportUsersAsync()
    {
        using var activity = Telemetry.Source.StartActivity("User.Export");
        
        var users = await _userManager.Users.ToListAsync();
        var exportList = new List<ExportUserDto>();

        foreach (var user in users)
        {
            var roles = await _roleService.GetUserRolesAsync(user.Id);
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            exportList.Add(new ExportUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                EmailConfirmedAt = user.EmailConfirmedAt,
                LastLoginAt = user.LastLoginAt,
                TwoFactorEnabled = user.TwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles,
                Permissions = permissions
            });
        }

        activity?.SetTag("users.exported", exportList.Count);
        activity?.SetStatus(ActivityStatusCode.Ok);

        // Audit log
        await _auditService.LogAsync("ExportUsers", Guid.Empty, $"Exported {exportList.Count} users", GetClientIpAddress());

        // Structured logging
        _logger.LogInformation("Exported {Count} users", exportList.Count);

        return exportList;
    }

    /// <summary>
    /// Imports users from a list of user data.
    /// </summary>
    /// <param name="users">The list of users to import.</param>
    /// <param name="skipExisting">Whether to skip users that already exist (true) or throw an exception (false).</param>
    /// <returns>A list of successfully imported users.</returns>
    public virtual async Task<List<ExportUserDto>> ImportUsersAsync(List<ImportUserDto> users, bool skipExisting = true)
    {
        using var activity = Telemetry.Source.StartActivity("User.Import");
        activity?.SetTag("users.to_import", users.Count);
        
        var importedUsers = new List<ExportUserDto>();
        var importedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        foreach (var importDto in users)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(importDto.Email);
                if (existingUser != null)
                {
                    if (skipExisting)
                    {
                        _logger.LogWarning("Skipping existing user: {Email}", importDto.Email);
                        skippedCount++;
                        continue;
                    }
                    throw new InfraExceptions.BusinessRuleException($"User with email {importDto.Email} already exists.");
                }

                // Generate random password if not provided
                var password = importDto.Password;
                if (string.IsNullOrWhiteSpace(password))
                {
                    password = GenerateRandomPassword();
                    _logger.LogInformation("Generated random password for user: {Email}", importDto.Email);
                }

                // Create new user
                var user = new AppUser
                {
                    Email = importDto.Email,
                    UserName = importDto.Email,
                    FullName = importDto.FullName,
                    AvatarUrl = importDto.AvatarUrl,
                    IsActive = importDto.IsActive,
                    EmailConfirmed = importDto.EmailConfirmed,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (importDto.EmailConfirmed)
                {
                    user.EmailConfirmedAt = DateTime.UtcNow;
                }

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
                    errors.Add($"Failed to import {importDto.Email}: {errorMessages}");
                    _logger.LogError("Failed to import user {Email}: {Errors}", importDto.Email, errorMessages);
                    continue;
                }

                // Assign roles
                if (importDto.Roles.Any())
                {
                    foreach (var roleName in importDto.Roles)
                    {
                        try
                        {
                            var roleExists = await _roleService.GetRoleByNameAsync(roleName);
                            if (roleExists != null)
                            {
                                await _userManager.AddToRoleAsync(user, roleName);
                            }
                            else
                            {
                                _logger.LogWarning("Role {Role} does not exist, skipping for user {Email}", roleName, importDto.Email);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to assign role {Role} to user {Email}", roleName, importDto.Email);
                        }
                    }
                }

                // Get roles and permissions for the imported user
                var roles = await _roleService.GetUserRolesAsync(user.Id);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                importedUsers.Add(new ExportUserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    EmailConfirmedAt = user.EmailConfirmedAt,
                    LastLoginAt = user.LastLoginAt,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles,
                    Permissions = permissions
                });

                importedCount++;

                // Publish event
                await _eventPublisher.PublishAsync(new UserRegisteredEvent
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName
                });

                // Audit log
                await _auditService.LogAsync("ImportUser", user.Id, $"User imported: {user.Email}", GetClientIpAddress());
            }
            catch (InfraExceptions.BusinessRuleException)
            {
                // Re-throw business rule exceptions if not skipping existing
                if (!skipExisting)
                {
                    throw;
                }
                errors.Add($"Business rule violation for {importDto.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing user {Email}", importDto.Email);
                errors.Add($"Error importing {importDto.Email}: {ex.Message}");
            }
        }

        activity?.SetTag("users.imported", importedCount);
        activity?.SetTag("users.skipped", skippedCount);
        activity?.SetTag("users.errors", errors.Count);
        activity?.SetStatus(errors.Any() ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

        // Audit log
        await _auditService.LogAsync("ImportUsers", Guid.Empty, 
            $"Imported {importedCount} users, skipped {skippedCount}, errors: {errors.Count}", 
            GetClientIpAddress());

        // Structured logging
        _logger.LogInformation("Imported {Imported} users, skipped {Skipped}, errors: {Errors}", 
            importedCount, skippedCount, errors.Count);

        if (errors.Any())
        {
            _logger.LogWarning("Import errors: {Errors}", string.Join("; ", errors));
        }

        return importedUsers;
    }

    /// <summary>
    /// Generates a random password for importing users without a password.
    /// </summary>
    /// <returns>A random password string.</returns>
    private string GenerateRandomPassword()
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var chars = new char[16];
        for (int i = 0; i < 16; i++)
        {
            chars[i] = validChars[random.Next(validChars.Length)];
        }
        return new string(chars);
    }

    /// <summary>
    /// Virtual hook called before user registration.
    /// </summary>
    /// <param name="dto">The registration data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnBeforeRegister(RegisterDto dto)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Virtual hook called after successful login.
    /// </summary>
    /// <param name="user">The user who logged in.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnAfterLogin(AppUser user)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context.
    /// </summary>
    /// <returns>The client IP address, or "Unknown" if not available.</returns>
    protected virtual string GetClientIpAddress()
    {
        if (_httpContextAccessor?.HttpContext == null)
        {
            return "Unknown";
        }

        var httpContext = _httpContextAccessor.HttpContext;

        // Check for forwarded IP address (from proxies/load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Maps an AppUser to UserProfileDto.
    /// </summary>
    private async Task<UserProfileDto> MapToProfileDtoAsync(AppUser user)
    {
        var roles = await _roleService.GetUserRolesAsync(user.Id);
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            EmailConfirmedAt = user.EmailConfirmedAt,
            LastLoginAt = user.LastLoginAt,
            TwoFactorEnabled = user.TwoFactorEnabled,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roles,
            Permissions = permissions
        };
    }
}
