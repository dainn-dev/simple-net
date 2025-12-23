using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
#if !DISABLE_MYSQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

namespace DainnProductEAVManagement.Contexts;

public class ProductCatalogDbContextFactory : IDesignTimeDbContextFactory<ProductCatalogDbContext>
{
    public ProductCatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductCatalogDbContext>();

        // Try to find appsettings.json in the API project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DainnUserManagement.API"))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        var dainnApplicationSection = configuration.GetSection("DainnApplication");
        var provider = dainnApplicationSection["Provider"] ?? "sqlite";
        // Use the same connection string as UserManagement (same database)
        var connectionString = dainnApplicationSection["ConnectionString"] ?? "Data Source=userdb.db";

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                optionsBuilder.UseSqlite(connectionString);
                break;
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "postgresql":
            case "npgsql":
            case "postgres":
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "mysql":
            case "mariadb":
#if !DISABLE_MYSQL
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
#else
                throw new InvalidOperationException("MySQL provider is not available. Use a different database provider.");
#endif
                break;
            case "inmemory":
                optionsBuilder.UseInMemoryDatabase(connectionString);
                break;
        }

        return new ProductCatalogDbContext(optionsBuilder.Options);
    }
}

