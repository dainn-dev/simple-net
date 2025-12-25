using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using System.Collections.Generic;
using DainnUser.PostgreSQL.Options;

namespace DainnUser.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for configuring Serilog structured logging.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog with structured logging, OpenTelemetry export, and span enrichment.
    /// Configuration is read from appsettings.json under the "Serilog" section.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder UseSerilogLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((ctx, config) =>
        {
            // Read Serilog options from configuration
            var serilogOptions = new SerilogOptions();
            ctx.Configuration.GetSection("Serilog").Bind(serilogOptions);

            config
                .ReadFrom.Configuration(ctx.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithSpan();

            // Only add OpenTelemetry sink if Serilog is enabled
            if (serilogOptions.Enabled)
            {
                config.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = serilogOptions.OtlpLogsEndpoint;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serilogOptions.ServiceName,
                        ["service.version"] = serilogOptions.ServiceVersion
                    };
                });
            }
        });
    }
}

