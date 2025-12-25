using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Helpers;
using DainnUser.PostgreSQL.Domain.Entities;
using DainnUser.PostgreSQL.Infrastructure.Persistence;
using DainnUser.PostgreSQL.Options;

namespace DainnUser.PostgreSQL.Infrastructure.Auth;

/// <summary>
/// Service for JWT token generation and refresh token management.
/// </summary>
public sealed class JwtService(AppDbContext context, IPermissionService permissionService, IRoleService roleService, UserManagementOptions options)
    : IAuthService
{

    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A task that completes with the JWT access token.</returns>
    public async Task<string> GenerateJwtAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Add roles
        var roles = await roleService.GetUserRolesAsync(user.Id);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add permissions
        var permissions = await permissionService.GetUserPermissionsAsync(user.Id);
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(options.JwtSettings.AccessTokenHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ipAddress">Optional IP address from which the token is being generated.</param>
    /// <param name="deviceInfo">Optional device information from the client.</param>
    /// <returns>A task that completes with the generated refresh token.</returns>
    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null, DeviceInfo? deviceInfo = null)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        var refreshToken = new RefreshToken
        {
            Token = tokenValue,
            UserId = userId,
            Expires = DateTime.UtcNow.AddDays(options.JwtSettings.RefreshTokenDays),
            IsRevoked = false,
            IpAddress = ipAddress,
            UserAgent = deviceInfo?.UserAgent,
            DeviceType = deviceInfo?.DeviceType,
            Browser = deviceInfo?.Browser,
            OperatingSystem = deviceInfo?.OperatingSystem,
            DeviceName = deviceInfo?.DeviceName
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return refreshToken;
    }

    /// <summary>
    /// Validates a refresh token and returns the associated user.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <returns>The user associated with the valid token, or null if invalid.</returns>
    public async Task<AppUser?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var token = await context.RefreshTokens
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
    public async Task RevokeAllTokensAsync(Guid userId)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await context.SaveChangesAsync();
    }
}

