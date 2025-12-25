using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Domain.Entities;

namespace DainnUser.PostgreSQL.Application.Interfaces;

/// <summary>
/// Interface for two-factor authentication operations.
/// </summary>
public interface ITwoFactorService
{
    /// <summary>
    /// Sets up two-factor authentication for a user.
    /// </summary>
    /// <param name="user">The user for whom to set up 2FA.</param>
    /// <returns>The two-factor setup information including secret and QR code URL.</returns>
    Task<TwoFactorSetupDto> Setup2FAAsync(AppUser user);

    /// <summary>
    /// Verifies a two-factor authentication code for a user.
    /// </summary>
    /// <param name="user">The user whose code is being verified.</param>
    /// <param name="code">The 6-digit TOTP code to verify.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    Task<bool> Verify2FACodeAsync(AppUser user, string code);
}

