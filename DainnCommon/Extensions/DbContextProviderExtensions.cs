using Microsoft.EntityFrameworkCore;
#if !DISABLE_MYSQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

namespace DainnCommon.Extensions;

/// <summary>
/// Extension methods for configuring database providers in Entity Framework Core.
/// </summary>
public static class DbContextProviderExtensions
{
    /// <summary>
    /// Configures the database provider based on the provider name and connection string.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    /// <param name="provider">The database provider name (sqlite, sqlserver, postgresql, mysql, inmemory).</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="configureProvider">Optional action to further configure the provider-specific options.</param>
    public static void ConfigureDatabaseProvider(
        this DbContextOptionsBuilder optionsBuilder,
        string provider,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureProvider = null)
    {
        var providerLower = provider.ToLowerInvariant();

        switch (providerLower)
        {
            case "sqlite":
                optionsBuilder.UseSqlite(connectionString);
                break;

            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
                break;

            case "postgresql":
            case "npgsql":
            case "postgres":
                optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
                break;

            case "mysql":
            case "mariadb":
#if !DISABLE_MYSQL
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mysqlOptions =>
                    {
                        mysqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
#else
                throw new InvalidOperationException(
                    "MySQL provider requires Pomelo.EntityFrameworkCore.MySql. " +
                    "Please use a different database provider (sqlite, sqlserver, postgresql) or enable MySQL support.");
#endif
                break;

            case "inmemory":
                optionsBuilder.UseInMemoryDatabase(connectionString);
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid or unsupported database provider: '{provider}'. " +
                    $"Supported providers are: sqlite, sqlserver, postgresql, npgsql, postgres, mysql, mariadb, inmemory. " +
                    $"Please set the provider configuration to one of these values.");
        }

        configureProvider?.Invoke(optionsBuilder);
    }

    /// <summary>
    /// Configures PostgreSQL UUID types for all Guid properties in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="setDefaultValue">Whether to set default value (gen_random_uuid()) for primary key Guid properties.</param>
    public static void ConfigurePostgreSqlUuids(this ModelBuilder modelBuilder, bool setDefaultValue = true)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(Guid) || p.ClrType == typeof(Guid?)))
            {
                property.SetColumnType("uuid");
                
                // Set default value for primary key Guid properties (non-nullable)
                if (setDefaultValue && property.ClrType == typeof(Guid) && !property.IsNullable && 
                    entityType.FindPrimaryKey()?.Properties.Contains(property) == true)
                {
                    property.SetDefaultValueSql("gen_random_uuid()");
                }
            }
        }
    }
}

