using Microsoft.EntityFrameworkCore;

namespace DainnProductEAV.PostgreSQL.Options;

/// <summary>
/// Configuration options for Product Catalog EAV system.
/// Can be bound from appsettings.json section "ProductCatalog".
/// </summary>
public class ProductCatalogOptions
{
    /// <summary>
    /// Database provider: sqlite, sqlserver, postgresql, npgsql, mysql, inmemory
    /// </summary>
    public string Provider { get; set; } = "sqlite";

    /// <summary>
    /// Connection string for the database.
    /// For InMemory provider, this is the database name.
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=productcatalog.db";

    /// <summary>
    /// Whether to automatically migrate the database on startup.
    /// </summary>
    public bool AutoMigrate { get; set; } = true;

    /// <summary>
    /// Whether to seed default attributes on first run.
    /// </summary>
    public bool SeedDefaultAttributes { get; set; } = true;

    /// <summary>
    /// Optional action to further configure the DbContext options.
    /// </summary>
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }
}
