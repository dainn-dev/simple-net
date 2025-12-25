namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for two-factor authentication setup information.
/// </summary>
public class TwoFactorSetupDto
{
    /// <summary>
    /// Gets or sets the secret key for TOTP authentication.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the QR code URL for easy setup with authenticator apps.
    /// </summary>
    public string QrCodeUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the createdAt
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

