namespace DainnUser.PostgreSQL.Options;

/// <summary>
/// Configuration settings for OpenTelemetry.
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets whether OpenTelemetry is enabled.
    /// Defaults to false to prevent silent failures when OTLP collector is not available.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the OTLP endpoint for traces (default: http://localhost:4317).
    /// </summary>
    public string OtlpTracesEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Gets or sets the service name for resource attributes (default: DainnUserManagement).
    /// </summary>
    public string ServiceName { get; set; } = "DainnUserManagement";

    /// <summary>
    /// Gets or sets the service version for resource attributes (default: 2.0.0).
    /// </summary>
    public string ServiceVersion { get; set; } = "2.0.0";

    /// <summary>
    /// Gets or sets whether to enable ASP.NET Core instrumentation.
    /// </summary>
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable HTTP client instrumentation.
    /// </summary>
    public bool EnableHttpClientInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable Entity Framework Core instrumentation.
    /// </summary>
    public bool EnableEntityFrameworkCoreInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable Prometheus exporter for metrics.
    /// </summary>
    public bool EnablePrometheusExporter { get; set; } = true;
}

