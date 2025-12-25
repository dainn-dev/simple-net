using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Options;
using DainnProductEAV.PostgreSQL.Repositories;
using DainnProductEAV.PostgreSQL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DainnCommon.Extensions;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace DainnProductEAV.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Product Catalog EAV services.
/// Supports multiple database providers: SQLite, SQL Server, PostgreSQL, MySQL, InMemory.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Product Catalog EAV services using configuration from appsettings.json.
    /// Reads from "DainnApplication" section, using Provider and ConnectionString (same database as UserManagement).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configureOptions">Optional action to further configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// // appsettings.json
    /// {
    ///   "DainnApplication": {
    ///     "Provider": "postgresql",
    ///     "ConnectionString": "Host=localhost;Database=userdb;Username=user;Password=pass",
    ///     "AutoMigrate": true,
    ///     "SeedDefaultAttributes": true
    ///   }
    /// }
    /// 
    /// // Program.cs
    /// builder.Services.AddProductCatalog(builder.Configuration);
    /// </example>
    public static IServiceCollection AddProductCatalog(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ProductCatalogOptions>? configureOptions = null)
    {
        var options = new ProductCatalogOptions();
        
        // Read Provider and ConnectionString from DainnApplication level (shared with UserManagement)
        var provider = configuration.GetValue<string>("DainnApplication:Provider");
        if (!string.IsNullOrWhiteSpace(provider))
        {
            options.Provider = provider;
        }
        
        // Use the same connection string as UserManagement (same database)
        var connectionString = configuration.GetValue<string>("DainnApplication:ConnectionString");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.ConnectionString = connectionString;
        }
        
        // Read ProductCatalog-specific settings
        options.AutoMigrate = configuration.GetValue<bool>("DainnApplication:AutoMigrate", true);
        options.SeedDefaultAttributes = configuration.GetValue<bool>("DainnApplication:SeedDefaultAttributes", true);
        
        configureOptions?.Invoke(options);

        return services.AddProductCatalogCore(options);
    }

    /// <summary>
    /// Adds the Product Catalog EAV services with explicit options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// builder.Services.AddProductCatalog(options =>
    /// {
    ///     options.Provider = "postgresql";
    ///     options.ConnectionString = "Host=localhost;Database=products;Username=user;Password=pass";
    /// });
    /// </example>
    public static IServiceCollection AddProductCatalog(
        this IServiceCollection services,
        Action<ProductCatalogOptions> configureOptions)
    {
        var options = new ProductCatalogOptions();
        configureOptions(options);

        return services.AddProductCatalogCore(options);
    }

    /// <summary>
    /// Adds the Product Catalog EAV services with a simple connection string (defaults to SQL Server).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProductCatalog(this IServiceCollection services, string connectionString)
    {
        return services.AddProductCatalog(options =>
        {
            options.Provider = "sqlserver";
            options.ConnectionString = connectionString;
        });
    }

    /// <summary>
    /// Core method that registers all Product Catalog services with the specified options.
    /// </summary>
    private static IServiceCollection AddProductCatalogCore(this IServiceCollection services, ProductCatalogOptions options)
    {
        // Validate configuration
        ValidateConfiguration(options);

        // Register DbContext with the appropriate provider
        services.AddDbContext<ProductCatalogDbContext>(dbOptions =>
        {
            dbOptions.ConfigureDatabaseProvider(options.Provider, options.ConnectionString, options.ConfigureDbContext);
        });

        // Register Memory Cache
        services.AddMemoryCache();

        // Register Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAttributeRepository, AttributeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Register Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAttributeService, AttributeService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Register options for injection
        services.AddSingleton(options);

        return services;
    }


    /// <summary>
    /// Validates the ProductCatalog configuration.
    /// </summary>
    private static void ValidateConfiguration(ProductCatalogOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            errors.Add(
                "The 'DainnApplication:Provider' configuration is required. " +
                "Please set it to one of: sqlite, sqlserver, postgresql, mysql, or inmemory.");
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            errors.Add(
                "The 'DainnApplication:ProductCatalog:ConnectionString' configuration is required. " +
                "Please provide a valid database connection string.");
        }

        if (errors.Any())
        {
            var errorMessage = "ProductCatalog configuration is invalid or missing required settings:\n\n" +
                             string.Join("\n\n", errors.Select((e, i) => $"{i + 1}. {e}")) +
                             "\n\nPlease add a 'DainnApplication' section to your appsettings.json with 'Provider' and 'ConnectionString' configuration.";

            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// Adds the Product Catalog EAV services with a custom repository implementation.
    /// </summary>
    /// <typeparam name="TProductRepository">Custom product repository type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProductCatalog<TProductRepository>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TProductRepository : class, IProductRepository
    {
        var options = new ProductCatalogOptions();
        
        // Read Provider and ConnectionString from DainnApplication level (shared with UserManagement)
        var provider = configuration.GetValue<string>("DainnApplication:Provider");
        if (!string.IsNullOrWhiteSpace(provider))
        {
            options.Provider = provider;
        }
        
        // Use the same connection string as UserManagement (same database)
        var connectionString = configuration.GetValue<string>("DainnApplication:ConnectionString");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.ConnectionString = connectionString;
        }
        
        // Read ProductCatalog-specific settings
        options.AutoMigrate = configuration.GetValue<bool>("DainnApplication:AutoMigrate", true);
        options.SeedDefaultAttributes = configuration.GetValue<bool>("DainnApplication:SeedDefaultAttributes", true);

        ValidateConfiguration(options);

        services.AddDbContext<ProductCatalogDbContext>(dbOptions =>
        {
            dbOptions.ConfigureDatabaseProvider(options.Provider, options.ConnectionString, options.ConfigureDbContext);
        });

        services.AddMemoryCache();

        // Register custom repository
        services.AddScoped<IProductRepository, TProductRepository>();
        services.AddScoped<IAttributeRepository, AttributeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAttributeService, AttributeService>();
        services.AddScoped<ICategoryService, CategoryService>();

        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Adds the Product Catalog EAV services with custom service implementations.
    /// </summary>
    public static IServiceCollection AddProductCatalogWithCustomServices<TProductService, TCategoryService, TAttributeService>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TProductService : class, IProductService
        where TCategoryService : class, ICategoryService
        where TAttributeService : class, IAttributeService
    {
        var options = new ProductCatalogOptions();
        
        // Read Provider and ConnectionString from DainnApplication level (shared with UserManagement)
        var provider = configuration.GetValue<string>("DainnApplication:Provider");
        if (!string.IsNullOrWhiteSpace(provider))
        {
            options.Provider = provider;
        }
        
        // Use the same connection string as UserManagement (same database)
        var connectionString = configuration.GetValue<string>("DainnApplication:ConnectionString");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.ConnectionString = connectionString;
        }
        
        // Read ProductCatalog-specific settings
        options.AutoMigrate = configuration.GetValue<bool>("DainnApplication:AutoMigrate", true);
        options.SeedDefaultAttributes = configuration.GetValue<bool>("DainnApplication:SeedDefaultAttributes", true);

        ValidateConfiguration(options);

        services.AddDbContext<ProductCatalogDbContext>(dbOptions =>
        {
            dbOptions.ConfigureDatabaseProvider(options.Provider, options.ConnectionString, options.ConfigureDbContext);
        });

        services.AddMemoryCache();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAttributeRepository, AttributeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Register custom Services
        services.AddScoped<IProductService, TProductService>();
        services.AddScoped<IAttributeService, TAttributeService>();
        services.AddScoped<ICategoryService, TCategoryService>();

        services.AddSingleton(options);

        return services;
    }
}
