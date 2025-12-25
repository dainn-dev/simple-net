using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
#if !DISABLE_MYSQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

namespace DainnUser.PostgreSQL.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Try to find appsettings.json in the API project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../DainnUserManagement.API"))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();

        var dainnApplicationSection = configuration.GetSection("DainnApplication");
        var provider = dainnApplicationSection["Provider"] ?? "sqlite";
        var connectionString = dainnApplicationSection["ConnectionString"] ?? "Data Source=app.db";

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
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case "mysql":
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

        return new AppDbContext(optionsBuilder.Options);
    }
}

