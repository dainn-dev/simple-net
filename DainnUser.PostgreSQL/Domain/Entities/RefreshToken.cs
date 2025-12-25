namespace DainnUser.PostgreSQL.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT token renewal.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the refresh token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration date and time of the refresh token.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets when the refresh token was created.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the refresh token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the user associated with this refresh token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the user associated with this refresh token.
    /// </summary>
    public AppUser User { get; set; } = null!;
}

