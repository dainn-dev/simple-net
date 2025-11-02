using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using DainnUserManagement.Application.Dtos;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Domain.Entities;

namespace DainnUserManagement.Application.Services;

/// <summary>
/// Service for OAuth2 external authentication operations.
/// </summary>
public class OAuth2Service : IOAuth2Service
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IAuthService _authService;
    private readonly ILogger<OAuth2Service> _logger;

    public OAuth2Service(
        UserManager<AppUser> userManager,
        IAuthService authService,
        ILogger<OAuth2Service> logger)
    {
        _userManager = userManager;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Processes an external login callback and returns authentication tokens and user profile.
    /// </summary>
    /// <param name="provider">The OAuth2 provider name (e.g., "Google", "Microsoft", "Facebook", "GitHub").</param>
    /// <param name="principal">The claims principal from the external authentication result.</param>
    /// <returns>The external login response containing tokens and user profile.</returns>
    public virtual async Task<ExternalLoginResponseDto> ProcessExternalLoginAsync(string provider, ClaimsPrincipal principal)
    {
        // Extract claims from the authentication result
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var providerKey = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(providerKey))
        {
            _logger.LogError("Missing required claims from {Provider} OAuth2", provider);
            throw new InvalidOperationException("Required claims (email, identifier) are missing");
        }

        // Handle external login (find or create user)
        var response = await HandleExternalLogin(provider, providerKey, email, name);

        return response;
    }

    /// <summary>
    /// Generic handler for external login processing.
    /// </summary>
    private async Task<ExternalLoginResponseDto> HandleExternalLogin(
        string provider,
        string providerKey,
        string email,
        string? fullName)
    {
        // Check if user already exists
        var existingUser = await FindUserByExternalLogin(provider, providerKey);

        AppUser user;
        bool isNewUser = false;

        if (existingUser != null)
        {
            user = existingUser;
            _logger.LogInformation("Existing user found for external login. Provider: {Provider}, Email: {Email}", provider, email);
        }
        else
        {
            // Check if user exists by email
            existingUser = await FindUserByEmail(email);
            
            if (existingUser != null)
            {
                // User exists but doesn't have this external login yet
                // Link the external login to the existing account
                user = existingUser;
                await AddExternalLoginToUser(user, provider, providerKey);
                _logger.LogInformation("Linked external login to existing user. Provider: {Provider}, Email: {Email}", provider, email);
            }
            else
            {
                // Create new user
                user = await CreateUserFromExternalLogin(email, fullName, provider, providerKey);
                isNewUser = true;
                _logger.LogInformation("Created new user from external login. Provider: {Provider}, Email: {Email}", provider, email);
            }
        }

        // Generate JWT tokens
        var accessToken = await _authService.GenerateJwtAsync(user);
        var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

        // Map user to profile DTO
        var userProfile = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return new ExternalLoginResponseDto
        {
            IsNewUser = isNewUser,
            Tokens = new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = 3600 // 1 hour
            },
            User = userProfile
        };
    }

    /// <summary>
    /// Finds a user by external login provider and key.
    /// </summary>
    private async Task<AppUser?> FindUserByExternalLogin(string provider, string providerKey)
    {
        var loginInfo = new UserLoginInfo(provider, providerKey, provider);
        var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
        return user;
    }

    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    private async Task<AppUser?> FindUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// Adds an external login to an existing user.
    /// </summary>
    private async Task AddExternalLoginToUser(AppUser user, string provider, string providerKey)
    {
        var loginInfo = new UserLoginInfo(provider, providerKey, provider);
        var result = await _userManager.AddLoginAsync(user, loginInfo);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add external login: {errors}");
        }
    }

    /// <summary>
    /// Creates a new user from external login information.
    /// </summary>
    private async Task<AppUser> CreateUserFromExternalLogin(string email, string? fullName, string provider, string providerKey)
    {
        var user = new AppUser
        {
            Email = email,
            UserName = email,
            FullName = fullName ?? email,
            EmailConfirmed = true,
            EmailConfirmedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create the user
        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Add external login
        var loginInfo = new UserLoginInfo(provider, providerKey, provider);
        var loginResult = await _userManager.AddLoginAsync(user, loginInfo);
        if (!loginResult.Succeeded)
        {
            var errors = string.Join(", ", loginResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add external login: {errors}");
        }

        return user;
    }
}

