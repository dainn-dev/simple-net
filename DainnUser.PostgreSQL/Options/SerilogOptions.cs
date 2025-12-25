namespace DainnUser.PostgreSQL.Options;

/// <summary>
/// Configuration settings for Serilog logging.
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// Gets or sets whether Serilog OpenTelemetry sink is enabled.
    /// Defaults to false to prevent silent failures when OTLP collector is not available.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the OTLP logs endpoint URL (default: http://localhost:4318/v1/logs).
    /// </summary>
    public string OtlpLogsEndpoint { get; set; } = "http://localhost:4318/v1/logs";

    /// <summary>
    /// Gets or sets the service name for resource attributes (default: DainnUserManagement).
    /// </summary>
    public string ServiceName { get; set; } = "DainnUserManagement";

    /// <summary>
    /// Gets or sets the service version for resource attributes (default: 2.0.0).
    /// </summary>
    public string ServiceVersion { get; set; } = "2.0.0";
}

