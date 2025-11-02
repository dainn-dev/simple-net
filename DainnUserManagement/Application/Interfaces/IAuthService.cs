using DainnUserManagement.Domain.Entities;

namespace DainnUserManagement.Application.Interfaces;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A task that completes with the JWT access token.</returns>
    Task<string> GenerateJwtAsync(AppUser user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task that completes with the generated refresh token.</returns>
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);

    /// <summary>
    /// Validates a refresh token and returns the associated user.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate.</param>
    /// <returns>The user associated with the valid token, or null if invalid.</returns>
    Task<AppUser?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAllTokensAsync(Guid userId);
}

