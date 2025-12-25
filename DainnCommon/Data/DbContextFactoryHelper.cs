using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
#if !DISABLE_MYSQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif
using DainnCommon.Extensions;

namespace DainnCommon.Data;

/// <summary>
/// Helper class for creating DbContext instances at design time.
/// </summary>
public static class DbContextFactoryHelper
{
    /// <summary>
    /// Creates a configuration builder that looks for appsettings.json in common locations.
    /// </summary>
    /// <param name="basePath">The base path to search for appsettings.json files.</param>
    /// <returns>A configured IConfiguration instance.</returns>
    public static IConfiguration BuildConfiguration(string? basePath = null)
    {
        var builder = new ConfigurationBuilder();

        if (!string.IsNullOrEmpty(basePath))
        {
            builder.SetBasePath(basePath);
        }
        else
        {
            // Try to find appsettings.json in the API project
            var currentDir = Directory.GetCurrentDirectory();
            var apiPath = Path.Combine(currentDir, "../DainnUserManagement.API");
            if (Directory.Exists(apiPath))
            {
                builder.SetBasePath(apiPath);
            }
            else
            {
                builder.SetBasePath(currentDir);
            }
        }

        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

        return builder.Build();
    }

    /// <summary>
    /// Gets database provider and connection string from configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name (default: "DainnApplication").</param>
    /// <param name="defaultProvider">The default provider if not found in configuration.</param>
    /// <param name="defaultConnectionString">The default connection string if not found in configuration.</param>
    /// <returns>A tuple containing the provider and connection string.</returns>
    public static (string Provider, string ConnectionString) GetDatabaseConfiguration(
        IConfiguration configuration,
        string sectionName = "DainnApplication",
        string defaultProvider = "sqlite",
        string defaultConnectionString = "Data Source=app.db")
    {
        var section = configuration.GetSection(sectionName);
        var provider = section["Provider"] ?? defaultProvider;
        var connectionString = section["ConnectionString"] ?? defaultConnectionString;

        return (provider, connectionString);
    }

    /// <summary>
    /// Configures a DbContext options builder with the specified provider and connection string.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="provider">The database provider name.</param>
    /// <param name="connectionString">The connection string.</param>
    public static void ConfigureDbContext<TContext>(
        DbContextOptionsBuilder<TContext> optionsBuilder,
        string provider,
        string connectionString)
        where TContext : DbContext
    {
        optionsBuilder.ConfigureDatabaseProvider(provider, connectionString);
    }
}

