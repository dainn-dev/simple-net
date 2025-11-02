using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Domain.Entities;
using DainnUserManagement.Infrastructure.Persistence;
using DainnUserManagement.Options;

namespace DainnUserManagement.Infrastructure.Auth;

/// <summary>
/// Service for JWT token generation and refresh token management.
/// </summary>
public class JwtService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;
    private readonly UserManagementOptions _options;

    public JwtService(AppDbContext context, IPermissionService permissionService, IRoleService roleService, UserManagementOptions options)
    {
        _context = context;
        _permissionService = permissionService;
        _roleService = roleService;
        _options = options;
    }

    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A task that completes with the JWT access token.</returns>
    public virtual async Task<string> GenerateJwtAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add roles
        var roles = await _roleService.GetUserRolesAsync(user.Id);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions
        var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_options.JwtSettings.AccessTokenHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that completes with the generated refresh token.</returns>
    public virtual async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = tokenValue,
            UserId = userId,
            Expires = DateTime.UtcNow.AddDays(_options.JwtSettings.RefreshTokenDays),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Validates a refresh token and returns the associated user.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <returns>The user associated with the valid token, or null if invalid.</returns>
    public virtual async Task<AppUser?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || token.IsRevoked || token.Expires < DateTime.UtcNow)
        {
            return null;
        }

        return token.User;
    }

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task RevokeAllTokensAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }
}

