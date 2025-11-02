using System.Diagnostics.Metrics;

namespace DainnUserManagement.Infrastructure.Metrics;

/// <summary>
/// Metrics for authentication operations.
/// </summary>
public static class AuthMetrics
{
    private static readonly Meter Meter = new("DainnUserManagement", "1.0.0");

    /// <summary>
    /// Counter for total login attempts with result label (success/failed).
    /// </summary>
    public static readonly Counter<long> LoginCounter = Meter.CreateCounter<long>(
        "auth.login.total",
        description: "Total number of login attempts");

    /// <summary>
    /// Counter for user registrations.
    /// </summary>
    public static readonly Counter<long> RegistrationCounter = Meter.CreateCounter<long>(
        "auth.registration.total",
        description: "Total number of user registrations");

    /// <summary>
    /// Counter for token refresh operations.
    /// </summary>
    public static readonly Counter<long> TokenRefreshCounter = Meter.CreateCounter<long>(
        "auth.token.refresh.total",
        description: "Total number of token refresh operations");
}

