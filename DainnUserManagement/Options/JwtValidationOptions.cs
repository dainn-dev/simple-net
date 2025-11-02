namespace DainnUserManagement.Options;

/// <summary>
/// Configuration settings for JWT token validation.
/// </summary>
public class JwtValidationOptions
{
    /// <summary>
    /// Gets or sets whether to validate the token issuer.
    /// </summary>
    public bool ValidateIssuer { get; set; } = false;

    /// <summary>
    /// Gets or sets the valid issuer (if ValidateIssuer is true).
    /// </summary>
    public string? ValidIssuer { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the token audience.
    /// </summary>
    public bool ValidateAudience { get; set; } = false;

    /// <summary>
    /// Gets or sets the valid audience (if ValidateAudience is true).
    /// </summary>
    public string? ValidAudience { get; set; }

    /// <summary>
    /// Gets or sets whether to validate the token lifetime.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the issuer signing key.
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Gets or sets the clock skew tolerance as a string (e.g., "00:00:00").
    /// This is converted to TimeSpan when used.
    /// </summary>
    public string ClockSkewString { get; set; } = "00:00:00";

    /// <summary>
    /// Gets the clock skew as a TimeSpan.
    /// </summary>
    public TimeSpan ClockSkew => TimeSpan.TryParse(ClockSkewString, out var result) ? result : TimeSpan.Zero;
}

