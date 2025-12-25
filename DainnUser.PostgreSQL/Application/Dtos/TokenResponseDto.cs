namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for authentication token response.
/// </summary>
public class TokenResponseDto
{
    /// <summary>
    /// Gets or sets the access token (JWT).
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; set; }
}

