using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DainnCommon.Data;

namespace DainnProductEAV.PostgreSQL.Contexts;

public class ProductCatalogDbContextFactory : IDesignTimeDbContextFactory<ProductCatalogDbContext>
{
    public ProductCatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductCatalogDbContext>();

        var configuration = DbContextFactoryHelper.BuildConfiguration();
        var (provider, connectionString) = DbContextFactoryHelper.GetDatabaseConfiguration(
            configuration,
            defaultProvider: "sqlite",
            defaultConnectionString: "Data Source=userdb.db");

        DbContextFactoryHelper.ConfigureDbContext(optionsBuilder, provider, connectionString);

        return new ProductCatalogDbContext(optionsBuilder.Options);
    }
}

