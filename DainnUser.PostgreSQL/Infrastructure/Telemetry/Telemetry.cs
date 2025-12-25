using System.Diagnostics;

namespace DainnUser.PostgreSQL.Infrastructure.Telemetry;

/// <summary>
/// Telemetry utilities for OpenTelemetry instrumentation.
/// </summary>
public static class Telemetry
{
    /// <summary>
    /// Gets the activity source for custom instrumentation.
    /// </summary>
    public static readonly ActivitySource Source = new("DainnUserManagement");
}

