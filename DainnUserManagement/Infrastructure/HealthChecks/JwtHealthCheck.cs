using Microsoft.Extensions.Diagnostics.HealthChecks;
using DainnUserManagement.Options;

namespace DainnUserManagement.Infrastructure.HealthChecks;

/// <summary>
/// Health check for JWT secret configuration validation.
/// </summary>
public class JwtHealthCheck : IHealthCheck
{
    private readonly UserManagementOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtHealthCheck"/> class.
    /// </summary>
    /// <param name="options">The user management options containing the JWT secret.</param>
    public JwtHealthCheck(UserManagementOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Checks the health of the JWT configuration by validating the secret length.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A health check result indicating the status of the JWT configuration.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Minimum recommended JWT secret length is 32 characters (256 bits)
        const int MinimumSecretLength = 32;

        if (string.IsNullOrWhiteSpace(_options.JwtSecret))
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "JWT secret is not configured or is empty."));
        }

        if (_options.JwtSecret.Length < MinimumSecretLength)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"JWT secret is too short ({_options.JwtSecret.Length} characters). " +
                $"Minimum recommended length is {MinimumSecretLength} characters for security."));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"JWT secret is configured correctly (length: {_options.JwtSecret.Length} characters)."));
    }
}

