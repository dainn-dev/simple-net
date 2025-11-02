namespace DainnUserManagement.Application.Dtos;

/// <summary>
/// DTO for external OAuth2 login request.
/// </summary>
public class ExternalLoginDto
{
    /// <summary>
    /// Gets or sets the OAuth2 provider name (e.g., "Google", "Apple").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authorization code or token from the provider.
    /// </summary>
    public string AuthorizationCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for external OAuth2 login callback response.
/// </summary>
public class ExternalLoginResponseDto
{
    /// <summary>
    /// Gets or sets whether the user is new or existing.
    /// </summary>
    public bool IsNewUser { get; set; }

    /// <summary>
    /// Gets or sets the JWT authentication tokens.
    /// </summary>
    public TokenResponseDto Tokens { get; set; } = new();

    /// <summary>
    /// Gets or sets the user profile information.
    /// </summary>
    public UserProfileDto User { get; set; } = new();
}

