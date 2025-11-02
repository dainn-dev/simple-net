using OtpNet;
using DainnUserManagement.Application.Dtos;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Domain.Entities;
using DainnUserManagement.Options;

namespace DainnUserManagement.Application.Services;

/// <summary>
/// Service for two-factor authentication using TOTP.
/// </summary>
public class TwoFactorService : ITwoFactorService
{
    private readonly string _applicationName;

    public TwoFactorService(UserManagementOptions options)
    {
        _applicationName = options.ApplicationName;
    }

    /// <summary>
    /// Sets up two-factor authentication for a user.
    /// </summary>
    /// <param name="user">The user for whom to set up 2FA.</param>
    /// <returns>The two-factor setup information including secret and QR code URL.</returns>
    public virtual Task<TwoFactorSetupDto> Setup2FAAsync(AppUser user)
    {
        // Generate a random secret key for TOTP
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Store the secret (not encrypted in this simple implementation - consider encryption in production)
        user.TwoFactorSecret = base32Secret;

        // Generate QR code URL
        var issuer = System.Net.WebUtility.UrlEncode(_applicationName);
        var accountName = System.Net.WebUtility.UrlEncode(user.Email ?? user.Id.ToString());
        var qrCodeUrl = $"otpauth://totp/{issuer}:{accountName}?secret={base32Secret}&issuer={issuer}";

        var setupDto = new TwoFactorSetupDto
        {
            Secret = base32Secret,
            QrCodeUrl = qrCodeUrl
        };

        return Task.FromResult(setupDto);
    }

    /// <summary>
    /// Verifies a two-factor authentication code for a user.
    /// </summary>
    /// <param name="user">The user whose code is being verified.</param>
    /// <param name="code">The 6-digit TOTP code to verify.</param>
    /// <returns>True if the code is valid, otherwise false.</returns>
    public virtual Task<bool> Verify2FACodeAsync(AppUser user, string code)
    {
        if (string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return Task.FromResult(false);
        }

        try
        {
            // Convert the base32 secret back to bytes
            var secretKeyBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);

            // Create TOTP instance
            var totp = new Totp(secretKeyBytes);

            // Verify the code with a time step tolerance
            bool isValid = totp.VerifyTotp(code, out long timeStepMatched, new VerificationWindow(2, 2));

            return Task.FromResult(isValid);
        }
        catch
        {
            // Invalid secret or code format
            return Task.FromResult(false);
        }
    }
}

