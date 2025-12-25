using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DainnProductEAV.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for database operations.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Migrates the database using EF Core migrations.
    /// For InMemory provider, uses EnsureCreated instead.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void MigrateProductCatalogDatabase(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ProductCatalogDbContext>();
            var options = services.GetRequiredService<ProductCatalogOptions>();
            var logger = services.GetService<ILogger<ProductCatalogDbContext>>();

            if (!options.AutoMigrate)
            {
                logger?.LogInformation("Auto-migration is disabled for ProductCatalog. Skipping database migration.");
                return;
            }

            var providerLower = options.Provider.ToLowerInvariant();

            if (providerLower == "inmemory")
            {
                // InMemory doesn't support migrations
                context.Database.EnsureCreated();
                logger?.LogInformation("InMemory database created for ProductCatalog.");
                return;
            }

            // Ensure database exists first
            if (!context.Database.CanConnect())
            {
                logger?.LogInformation("Database does not exist. It will be created during migration.");
            }

            // Ensure the migration history table exists before calling Migrate()
            // Migrate() internally queries this table, so it must exist first
            EnsureMigrationHistoryTable(context, logger);

            // Now call Migrate() - it can safely query the history table
            logger?.LogInformation("Applying ProductCatalog database migrations...");
            context.Database.Migrate();
            logger?.LogInformation("ProductCatalog database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILogger<ProductCatalogDbContext>>();
            logger?.LogError(ex, "An error occurred while migrating the ProductCatalog database. Error: {Error}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Async version of MigrateProductCatalogDatabase.
    /// </summary>
    public static async Task MigrateProductCatalogDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ProductCatalogDbContext>();
            var options = services.GetRequiredService<ProductCatalogOptions>();
            var logger = services.GetService<ILogger<ProductCatalogDbContext>>();

            if (!options.AutoMigrate)
            {
                logger?.LogInformation("Auto-migration is disabled for ProductCatalog. Skipping database migration.");
                return;
            }

            var providerLower = options.Provider.ToLowerInvariant();

            if (providerLower == "inmemory")
            {
                await context.Database.EnsureCreatedAsync();
                logger?.LogInformation("InMemory database created for ProductCatalog.");
                return;
            }

            // Ensure database exists first
            if (!await context.Database.CanConnectAsync())
            {
                logger?.LogInformation("Database does not exist. It will be created during migration.");
            }

            // Ensure the migration history table exists before calling MigrateAsync()
            // MigrateAsync() internally queries this table, so it must exist first
            EnsureMigrationHistoryTable(context, logger);

            // Now call MigrateAsync() - it can safely query the history table
            logger?.LogInformation("Applying ProductCatalog database migrations...");
            await context.Database.MigrateAsync();
            logger?.LogInformation("ProductCatalog database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILogger<ProductCatalogDbContext>>();
            logger?.LogError(ex, "An error occurred while migrating the ProductCatalog database. Error: {Error}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Ensures the __EFMigrationsHistory table exists. Creates it if it doesn't exist.
    /// </summary>
    private static void EnsureMigrationHistoryTable(ProductCatalogDbContext context, ILogger? logger)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            var providerName = context.Database.ProviderName ?? "";
            bool tableExists = false;

            // Check if table exists
            using var checkCommand = connection.CreateCommand();
            
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                checkCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("SqlServer"))
            {
                checkCommand.CommandText = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("Sqlite"))
            {
                checkCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                checkCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else
            {
                // Unknown provider, let Migrate() handle it
                return;
            }

            var result = checkCommand.ExecuteScalar();
            tableExists = result != null && Convert.ToInt64(result) > 0;

            if (!tableExists)
            {
                logger?.LogInformation("Creating __EFMigrationsHistory table...");
                
                // Use EF Core's history repository to get the create script
                var historyRepository = context.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IHistoryRepository>();
                var createScript = historyRepository.GetCreateScript();
                
                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = createScript;
                createCommand.ExecuteNonQuery();
                
                logger?.LogInformation("__EFMigrationsHistory table created successfully.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not ensure migration history table exists. Migrate() will attempt to create it.");
            // Continue - Migrate() might still work
        }
    }

    /// <summary>
    /// Checks if the ProductCatalog tables already exist in the database.
    /// </summary>
    private static bool CheckTablesExist(ProductCatalogDbContext context, string provider, ILogger? logger)
    {
        try
        {
            if (!context.Database.CanConnect())
            {
                logger?.LogDebug("Cannot connect to database.");
                return false;
            }

            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();

            command.CommandText = provider switch
            {
                "postgresql" or "npgsql" or "postgres" => @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = 'product_entity';",
                "sqlserver" => @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'product_entity';",
                "mysql" or "mariadb" => @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                    AND table_name = 'product_entity';",
                "sqlite" => "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='product_entity';",
                _ => "SELECT 0;"
            };

            var result = command.ExecuteScalar();
            var exists = result != null && Convert.ToInt64(result) > 0;
            logger?.LogDebug("CheckTablesExist: product_entity = {Exists}", exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Could not check if tables exist.");
            return false;
        }
    }

    /// <summary>
    /// Checks if the __EFMigrationsHistory table exists.
    /// </summary>
    private static bool CheckHistoryTableExists(ProductCatalogDbContext context, ILogger? logger)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            var providerName = context.Database.ProviderName ?? "";

            using var checkTableCommand = connection.CreateCommand();
            
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("SqlServer"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("Sqlite"))
            {
                checkTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else
            {
                return false;
            }

            var tableResult = checkTableCommand.ExecuteScalar();
            var tableExists = tableResult != null && Convert.ToInt64(tableResult) > 0;
            logger?.LogDebug("__EFMigrationsHistory table exists: {Exists}", tableExists);
            return tableExists;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Could not check if migration history table exists.");
            return false;
        }
    }

    /// <summary>
    /// Checks if the __EFMigrationsHistory table has any records.
    /// </summary>
    private static bool CheckHistoryTableHasRecords(ProductCatalogDbContext context, ILogger? logger)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            var providerName = context.Database.ProviderName ?? "";

            // First check if history table exists
            using var checkTableCommand = connection.CreateCommand();
            
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("SqlServer"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("Sqlite"))
            {
                checkTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else
            {
                return false;
            }

            var tableResult = checkTableCommand.ExecuteScalar();
            var tableExists = tableResult != null && Convert.ToInt64(tableResult) > 0;
            
            if (!tableExists)
            {
                logger?.LogDebug("__EFMigrationsHistory table does not exist.");
                return false;
            }

            // Check if it has records
            using var countCommand = connection.CreateCommand();
            
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                countCommand.CommandText = @"SELECT COUNT(*) FROM ""__EFMigrationsHistory"";";
            }
            else if (providerName.Contains("SqlServer"))
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM [__EFMigrationsHistory];";
            }
            else if (providerName.Contains("Sqlite"))
            {
                countCommand.CommandText = @"SELECT COUNT(*) FROM ""__EFMigrationsHistory"";";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM `__EFMigrationsHistory`;";
            }

            var countResult = countCommand.ExecuteScalar();
            var hasRecords = countResult != null && Convert.ToInt64(countResult) > 0;
            logger?.LogDebug("__EFMigrationsHistory has records: {HasRecords}", hasRecords);
            return hasRecords;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Could not check migration history.");
            return false;
        }
    }

    /// <summary>
    /// Marks a migration as applied in the __EFMigrationsHistory table.
    /// </summary>
    private static void MarkMigrationAsApplied(ProductCatalogDbContext context, string migrationId, ILogger? logger)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            var providerName = context.Database.ProviderName ?? "";

            // Ensure history table exists
            var historyRepository = context.GetService<IHistoryRepository>();
            
            using var checkTableCommand = connection.CreateCommand();
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("SqlServer"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = '__EFMigrationsHistory';";
            }
            else if (providerName.Contains("Sqlite"))
            {
                checkTableCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                checkTableCommand.CommandText = @"
                    SELECT COUNT(*) FROM information_schema.tables 
                    WHERE table_schema = DATABASE() 
                    AND table_name = '__EFMigrationsHistory';";
            }

            var tableResult = checkTableCommand.ExecuteScalar();
            var tableExists = tableResult != null && Convert.ToInt64(tableResult) > 0;

            if (!tableExists)
            {
                var createScript = historyRepository.GetCreateScript();
                using var createCommand = connection.CreateCommand();
                createCommand.CommandText = createScript;
                createCommand.ExecuteNonQuery();
                logger?.LogDebug("Created __EFMigrationsHistory table.");
            }

            // Check if migration already recorded
            using var checkCommand = connection.CreateCommand();
            
            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                checkCommand.CommandText = $@"SELECT COUNT(*) FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '{migrationId}';";
            }
            else if (providerName.Contains("SqlServer"))
            {
                checkCommand.CommandText = $"SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{migrationId}';";
            }
            else if (providerName.Contains("Sqlite"))
            {
                checkCommand.CommandText = $@"SELECT COUNT(*) FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = '{migrationId}';";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                checkCommand.CommandText = $"SELECT COUNT(*) FROM `__EFMigrationsHistory` WHERE `MigrationId` = '{migrationId}';";
            }

            var existsResult = checkCommand.ExecuteScalar();
            if (existsResult != null && Convert.ToInt64(existsResult) > 0)
            {
                logger?.LogDebug("Migration '{MigrationId}' already in history.", migrationId);
                return;
            }

            // Insert migration record
            using var insertCommand = connection.CreateCommand();
            var efVersion = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "8.0.0";

            if (providerName.Contains("Npgsql") || providerName.Contains("PostgreSQL"))
            {
                insertCommand.CommandText = $@"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                    VALUES ('{migrationId}', '{efVersion}');";
            }
            else if (providerName.Contains("SqlServer"))
            {
                insertCommand.CommandText = $@"
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('{migrationId}', '{efVersion}');";
            }
            else if (providerName.Contains("Sqlite"))
            {
                insertCommand.CommandText = $@"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                    VALUES ('{migrationId}', '{efVersion}');";
            }
            else if (providerName.Contains("MySql") || providerName.Contains("Pomelo"))
            {
                insertCommand.CommandText = $@"
                    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                    VALUES ('{migrationId}', '{efVersion}');";
            }

            insertCommand.ExecuteNonQuery();
            logger?.LogDebug("Marked migration '{MigrationId}' as applied.", migrationId);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Could not mark migration '{MigrationId}' as applied.", migrationId);
        }
    }

    /// <summary>
    /// Seeds initial data into the database if not already present.
    /// </summary>
    public static void SeedProductCatalogData(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<ProductCatalogOptions>();
        var logger = scope.ServiceProvider.GetService<ILogger<ProductCatalogDbContext>>();

        if (!options.SeedDefaultAttributes)
        {
            logger?.LogInformation("Seeding default attributes is disabled. Skipping.");
            return;
        }

        logger?.LogInformation("ProductCatalog seed data applied (configured in DbContext migrations).");
    }

    /// <summary>
    /// Async version of SeedProductCatalogData.
    /// </summary>
    public static async Task SeedProductCatalogDataAsync(this IServiceProvider serviceProvider)
    {
        await Task.Run(() => serviceProvider.SeedProductCatalogData());
    }
}
