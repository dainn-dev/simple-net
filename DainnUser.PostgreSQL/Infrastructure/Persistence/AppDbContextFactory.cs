using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DainnCommon.Data;

namespace DainnUser.PostgreSQL.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var configuration = DbContextFactoryHelper.BuildConfiguration();
        var (provider, connectionString) = DbContextFactoryHelper.GetDatabaseConfiguration(
            configuration,
            defaultProvider: "sqlite",
            defaultConnectionString: "Data Source=app.db");

        DbContextFactoryHelper.ConfigureDbContext(optionsBuilder, provider, connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}

