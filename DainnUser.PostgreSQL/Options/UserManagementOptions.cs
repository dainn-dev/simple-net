using Microsoft.EntityFrameworkCore;

namespace DainnUser.PostgreSQL.Options;

public class UserManagementOptions
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
    public string JwtSecret { get; set; } = string.Empty;
    public Type? CustomUserServiceType { get; set; }
    public bool Enable2FA { get; set; }

    /// <summary>
    /// Gets or sets the JWT settings.
    /// </summary>
    public JwtSettings JwtSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the application name used for 2FA QR codes.
    /// </summary>
    public string ApplicationName { get; set; } = "DainnUserManagement";

    /// <summary>
    /// Gets or sets the security settings for login lockout.
    /// </summary>
    public SecuritySettings SecuritySettings { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to automatically apply database migrations on startup.
    /// </summary>
    public bool AutoMigrateDatabase { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to seed the database with default admin role and user.
    /// </summary>
    public bool SeedDefaultAdmin { get; set; } = false;

    /// <summary>
    /// Gets or sets the default admin user email.
    /// </summary>
    public string DefaultAdminEmail { get; set; } = "admin@example.com";

    /// <summary>
    /// Gets or sets the default admin user password.
    /// </summary>
    public string DefaultAdminPassword { get; set; } = "Admin@123!";

    /// <summary>
    /// Gets or sets the CORS configuration settings.
    /// </summary>
    public CorsSettings CorsSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to enforce HTTPS redirection.
    /// </summary>
    public bool EnforceHttps { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable HSTS (HTTP Strict Transport Security).
    /// </summary>
    public bool EnableHsts { get; set; } = false;

    /// <summary>
    /// Gets or sets the JWT validation settings.
    /// </summary>
    public JwtValidationOptions JwtValidation { get; set; } = new();

    /// <summary>
    /// Gets or sets the OAuth2 external authentication settings.
    /// </summary>
    public OAuth2Settings OAuth2Settings { get; set; } = new();
}

/// <summary>
/// Configuration settings for JWT tokens.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the access token expiration time in hours.
    /// </summary>
    public int AccessTokenHours { get; set; } = 1;

    /// <summary>
    /// Gets or sets the refresh token expiration time in days.
    /// </summary>
    public int RefreshTokenDays { get; set; } = 7;
}

/// <summary>
/// Configuration settings for security features like login lockout and rate limiting.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Gets or sets the maximum number of failed login attempts before account lockout.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Gets or sets the duration in minutes for which an account is locked after exceeding max failed attempts.
    /// </summary>
    public int LockoutDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether rate limiting is enabled.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Gets or sets the rate limit for login and register endpoints (requests per minute).
    /// </summary>
    public int RateLimitRequestsPerMinute { get; set; } = 10;
}

/// <summary>
/// Configuration settings for CORS (Cross-Origin Resource Sharing).
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// Gets or sets whether CORS is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the allowed origins (comma-separated or array).
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = new();

    /// <summary>
    /// Gets or sets the allowed methods.
    /// </summary>
    public List<string> AllowedMethods { get; set; } = new() { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

    /// <summary>
    /// Gets or sets the allowed headers.
    /// </summary>
    public List<string> AllowedHeaders { get; set; } = new() { "Content-Type", "Authorization", "X-Requested-With" };

    /// <summary>
    /// Gets or sets whether to allow credentials.
    /// </summary>
    public bool AllowCredentials { get; set; } = true;
}

/// <summary>
/// Configuration settings for OAuth2 external authentication providers (Google, Microsoft, Facebook, GitHub, Apple, etc.).
/// </summary>
public class OAuth2Settings
{
    /// <summary>
    /// Gets or sets the Google OAuth2 configuration.
    /// </summary>
    public GoogleOAuth2Settings Google { get; set; } = new();

    /// <summary>
    /// Gets or sets the Microsoft OAuth2 configuration.
    /// </summary>
    public MicrosoftOAuth2Settings Microsoft { get; set; } = new();

    /// <summary>
    /// Gets or sets the Facebook OAuth2 configuration.
    /// </summary>
    public FacebookOAuth2Settings Facebook { get; set; } = new();

    /// <summary>
    /// Gets or sets the GitHub OAuth2 configuration.
    /// </summary>
    public GitHubOAuth2Settings GitHub { get; set; } = new();

    /// <summary>
    /// Gets or sets the Apple OAuth2 configuration.
    /// </summary>
    public AppleOAuth2Settings Apple { get; set; } = new();
}

/// <summary>
/// Configuration settings for Google OAuth2 authentication.
/// </summary>
public class GoogleOAuth2Settings
{
    /// <summary>
    /// Gets or sets whether Google authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Google OAuth2 client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Google OAuth2 client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for Microsoft OAuth2 authentication.
/// </summary>
public class MicrosoftOAuth2Settings
{
    /// <summary>
    /// Gets or sets whether Microsoft authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Microsoft OAuth2 client ID (Application ID).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Microsoft OAuth2 client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for Facebook OAuth2 authentication.
/// </summary>
public class FacebookOAuth2Settings
{
    /// <summary>
    /// Gets or sets whether Facebook authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Facebook OAuth2 App ID.
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Facebook OAuth2 App Secret.
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for GitHub OAuth2 authentication.
/// </summary>
public class GitHubOAuth2Settings
{
    /// <summary>
    /// Gets or sets whether GitHub authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the GitHub OAuth2 Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub OAuth2 Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Configuration settings for Apple OAuth2 authentication.
/// </summary>
public class AppleOAuth2Settings
{
    /// <summary>
    /// Gets or sets whether Apple authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Apple OAuth2 client ID (Service ID).
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Apple OAuth2 key ID (for signing).
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Apple OAuth2 team ID.
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the Apple private key file (PKCS8 format).
    /// </summary>
    public string PrivateKeyPath { get; set; } = string.Empty;
}

