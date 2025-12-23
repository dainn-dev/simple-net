# DainnProductEAVManagement

A .NET 8 class library implementing the **Entity-Attribute-Value (EAV)** pattern for product catalog management, similar to Magento's architecture. This library provides a flexible, extensible product catalog system that allows adding custom attributes without modifying the database schema.

## Features

- ✅ **EAV Pattern** - Dynamic attributes stored in type-specific tables (varchar, int, decimal, text, datetime)
- ✅ **Multi-Store Support** - Store-specific attribute values with global fallback
- ✅ **Multi-Provider Support** - SQLite, SQL Server, PostgreSQL, MySQL, InMemory
- ✅ **Attribute Sets & Groups** - Organize attributes like Magento
- ✅ **Category Management** - Hierarchical categories with nested set model
- ✅ **Inventory Management** - Stock tracking per product
- ✅ **Tier Pricing** - Customer group and website-specific pricing
- ✅ **Media Gallery** - Product images and videos
- ✅ **Product Relations** - Configurable, grouped, related, upsell, crosssell products
- ✅ **Caching** - Built-in memory caching for attribute metadata
- ✅ **Extensible** - Easy to extend with custom repositories and services
- ✅ **Guid Primary Keys** - All entities use Guid (UUID) for primary keys
- ✅ **PascalCase Tables** - Consistent PascalCase naming convention for all database tables

## Table of Contents

1. [Installation](#installation)
2. [Quick Start](#quick-start)
3. [Configuration](#configuration)
4. [Database Providers](#database-providers)
5. [Sample Data](#sample-data)
6. [Usage Examples](#usage-examples)
7. [API Reference](#api-reference)
8. [Extending the Library](#extending-the-library)
9. [Database Schema](#database-schema)

---

## Installation

### 1. Add Project Reference

If you have the source code:

```bash
dotnet add reference ../DainnProductEAVManagement/DainnProductEAVManagement.csproj
```

Or add as a NuGet package (when published):

```bash
dotnet add package DainnProductEAVManagement
```

### 2. Required Dependencies

The library includes these EF Core providers:
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Sqlite`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Pomelo.EntityFrameworkCore.MySql`
- `Microsoft.EntityFrameworkCore.InMemory`

---

## Quick Start

### Step 1: Add Configuration to `appsettings.json`

```json
{
  "ProductCatalog": {
    "Provider": "sqlite",
    "ConnectionString": "Data Source=productcatalog.db",
    "AutoMigrate": true,
    "SeedDefaultAttributes": true
  }
}
```

### Step 2: Register Services in `Program.cs`

```csharp
using DainnProductEAVManagement.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Product Catalog services
builder.Services.AddProductCatalog(builder.Configuration);

var app = builder.Build();

// Apply database migrations
app.Services.MigrateProductCatalogDatabase();

app.Run();
```

### Step 3: Use the Services

```csharp
using DainnProductEAVManagement.Services;

public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct()
    {
        // Create a product
        var product = await _productService.CreateProductAsync("SKU-001", "simple", Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Set attributes
        await _productService.SetAttributeAsync(product.EntityId, "name", "My Product");
        await _productService.SetAttributeAsync(product.EntityId, "price", 29.99m);
        await _productService.SetAttributeAsync(product.EntityId, "status", 1);

        return Ok(product);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _productService.GetProductAsync(id);
        var name = await _productService.GetAttributeAsync<string>(id, "name");
        var price = await _productService.GetAttributeAsync<decimal>(id, "price");

        return Ok(new { product, name, price });
    }
}
```

---

## Configuration

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Provider` | string | `"sqlite"` | Database provider: `sqlite`, `sqlserver`, `postgresql`, `mysql`, `inmemory` |
| `ConnectionString` | string | Required | Database connection string |
| `AutoMigrate` | bool | `true` | Automatically create/migrate database on startup |
| `SeedDefaultAttributes` | bool | `true` | Seed default product attributes |

### Example Configurations

#### SQLite
```json
{
  "ProductCatalog": {
    "Provider": "sqlite",
    "ConnectionString": "Data Source=products.db"
  }
}
```

#### SQL Server
```json
{
  "ProductCatalog": {
    "Provider": "sqlserver",
    "ConnectionString": "Server=localhost;Database=ProductCatalog;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

#### PostgreSQL
```json
{
  "ProductCatalog": {
    "Provider": "postgresql",
    "ConnectionString": "Host=localhost;Database=productcatalog;Username=postgres;Password=password"
  }
}
```

#### MySQL
```json
{
  "ProductCatalog": {
    "Provider": "mysql",
    "ConnectionString": "Server=localhost;Database=productcatalog;User=root;Password=password"
  }
}
```

#### InMemory (for Testing)
```json
{
  "ProductCatalog": {
    "Provider": "inmemory",
    "ConnectionString": "ProductCatalogTestDb"
  }
}
```

---

## Database Providers

### Programmatic Configuration

You can also configure the provider programmatically:

```csharp
// Using configuration
builder.Services.AddProductCatalog(builder.Configuration);

// Or with explicit options
builder.Services.AddProductCatalog(options =>
{
    options.Provider = "postgresql";
    options.ConnectionString = "Host=localhost;Database=products;...";
    options.AutoMigrate = true;
});

// Or with custom DbContext options
builder.Services.AddProductCatalog(dbOptions =>
{
    dbOptions.UseNpgsql("your-connection-string", npgsql =>
    {
        npgsql.EnableRetryOnFailure();
        npgsql.CommandTimeout(30);
    });
});
```

---

## Sample Data

The library includes a sample data seeder for testing and demonstration purposes. It creates realistic sample products, categories, and attribute values.

### Seeding Sample Data

```csharp
using DainnProductEAVManagement.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProductCatalog(builder.Configuration);

var app = builder.Build();

// Apply database migrations
app.Services.MigrateProductCatalogDatabase();

// Seed sample data (optional - for testing/demo)
app.Services.SeedSampleData();

app.Run();
```

Or use the async version:

```csharp
await app.Services.SeedSampleDataAsync();
```

### Sample Categories

The seeder creates a hierarchical category structure:

```
Electronics/
├── Computers & Laptops
├── Smartphones & Tablets
└── Audio & Headphones

Clothing/
├── Men's Clothing
└── Women's Clothing

Home & Garden/
├── Furniture
└── Home Decor
```

### Sample Products

| SKU | Name | Category | Price |
|-----|------|----------|-------|
| LAPTOP-001 | ProBook Laptop 15.6" | Computers & Laptops | $1,299.99 |
| PHONE-001 | Galaxy Ultra Pro 5G | Smartphones & Tablets | $999.99 |
| TABLET-001 | iPad Pro 12.9" | Smartphones & Tablets | $1,099.00 |
| AUDIO-001 | NoiseCancel Pro Headphones | Audio & Headphones | $349.99 |
| AUDIO-002 | BoomBox Portable Speaker | Audio & Headphones | $199.99 |
| SHIRT-M-001 | Classic Oxford Button-Down Shirt | Men's Clothing | $79.99 |
| PANTS-M-001 | Slim Fit Chino Pants | Men's Clothing | $89.99 |
| DRESS-W-001 | Elegant Summer Maxi Dress | Women's Clothing | $129.99 |
| BLOUSE-W-001 | Silk Blend Blouse | Women's Clothing | $99.99 |
| SOFA-001 | Modern 3-Seater Sofa | Furniture | $899.99 |
| TABLE-001 | Solid Oak Dining Table | Furniture | $1,299.99 |
| LAMP-001 | Scandinavian Floor Lamp | Home Decor | $149.99 |
| VASE-001 | Ceramic Artisan Vase | Home Decor | $59.99 |
| CUSHION-001 | Velvet Throw Cushion Set | Home Decor | $49.99 |

### Sample Product Data Includes

Each sample product comes with:

- ✅ **Core Attributes** - name, description, short_description, url_key
- ✅ **Pricing** - price, special_price (where applicable)
- ✅ **Status** - Enabled and visible in catalog/search
- ✅ **Weight** - Shipping weight
- ✅ **SEO** - meta_title, meta_description
- ✅ **Inventory** - Stock quantity and in-stock status
- ✅ **Category Assignment** - Assigned to appropriate category

### Note

The sample data seeder checks if products already exist before seeding. It will skip seeding if the database already contains products to avoid duplicates.

---

## Usage Examples

### Working with Products

```csharp
public class ProductExamples
{
    private readonly IProductService _productService;

    public ProductExamples(IProductService productService)
    {
        _productService = productService;
    }

    // Create a product
    public async Task CreateProduct()
    {
        var product = await _productService.CreateProductAsync(
            sku: "SHIRT-001",
            typeId: "simple",      // simple, configurable, bundle, grouped
            attributeSetId: Guid.Parse("11111111-1111-1111-1111-111111111111")  // Default attribute set
        );

        // Set multiple attributes at once
        await _productService.SetAttributesAsync(product.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Blue Cotton Shirt",
            ["description"] = "A comfortable blue cotton shirt",
            ["price"] = 49.99m,
            ["status"] = 1,        // 1 = Enabled, 0 = Disabled
            ["visibility"] = 4,    // 4 = Catalog & Search
            ["weight"] = 0.5m,
            ["color"] = 1,         // Color option ID
            ["size"] = 2           // Size option ID
        });
    }

    // Get product with attributes
    public async Task<object> GetProductDetails(Guid productId, int storeId = 0)
    {
        var product = await _productService.GetProductAsync(productId, storeId);
        
        // Get individual attributes
        var name = await _productService.GetAttributeAsync<string>(productId, "name", storeId);
        var price = await _productService.GetAttributeAsync<decimal>(productId, "price", storeId);
        var description = await _productService.GetAttributeAsync<string>(productId, "description", storeId);

        return new { product, name, price, description };
    }

    // Update product
    public async Task UpdateProduct(Guid productId)
    {
        var product = await _productService.GetProductAsync(productId);
        product!.Sku = "SHIRT-001-UPDATED";
        await _productService.UpdateProductAsync(product);

        // Update attributes
        await _productService.SetAttributeAsync(productId, "price", 39.99m);
    }

    // Search products
    public async Task<IEnumerable<object>> SearchProducts(string query)
    {
        return await _productService.SearchProductsAsync(query, storeId: 0, page: 1, pageSize: 20);
    }

    // Delete product
    public async Task DeleteProduct(Guid productId)
    {
        await _productService.DeleteProductAsync(productId);
    }
}
```

### Working with Categories

```csharp
public class CategoryExamples
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;

    public CategoryExamples(ICategoryService categoryService, IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }

    // Create category hierarchy
    public async Task CreateCategories()
    {
        // Create root category
        var clothing = await _categoryService.CreateCategoryAsync("Clothing", parentId: null);

        // Create child categories
        var shirts = await _categoryService.CreateCategoryAsync("Shirts", parentId: clothing.EntityId);
        var pants = await _categoryService.CreateCategoryAsync("Pants", parentId: clothing.EntityId);
        var tshirts = await _categoryService.CreateCategoryAsync("T-Shirts", parentId: shirts.EntityId);
    }

    // Note: All IDs (productId, categoryId, attributeId, etc.) are of type Guid

    // Get category tree
    public async Task<IEnumerable<object>> GetCategoryTree()
    {
        return await _categoryService.GetCategoryTreeAsync(rootId: null);
    }

    // Assign product to category
    public async Task AssignProductToCategory(Guid productId, Guid categoryId)
    {
        await _productService.AssignProductToCategoryAsync(productId, categoryId, position: 0);
    }

    // Get products in category
    public async Task<IEnumerable<object>> GetProductsInCategory(Guid categoryId)
    {
        return await _productService.GetProductsByCategoryAsync(categoryId, storeId: 0, page: 1, pageSize: 20);
    }

    // Move category
    public async Task MoveCategory(Guid categoryId, Guid? newParentId)
    {
        await _categoryService.MoveCategoryAsync(categoryId, newParentId, position: 0);
    }
}
```

### Working with Attributes

```csharp
public class AttributeExamples
{
    private readonly IAttributeService _attributeService;

    public AttributeExamples(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    // Create custom attribute
    public async Task CreateCustomAttribute()
    {
        var attribute = await _attributeService.CreateAttributeAsync(
            attributeCode: "brand",
            backendType: "varchar",      // varchar, int, decimal, text, datetime
            frontendInput: "select",     // text, textarea, select, multiselect, boolean, date, price
            frontendLabel: "Brand",
            isRequired: false,
            isSearchable: true,
            isFilterable: true,
            attributeGroupId: Guid.Parse("11111111-1111-1111-1111-111111111111")  // General group
        );
    }

    // Create attribute set (e.g., for Electronics)
    public async Task CreateAttributeSet()
    {
        // Create the set
        var attributeSet = await _attributeService.CreateAttributeSetAsync("Electronics");

        // Create groups within the set
        var specsGroup = await _attributeService.CreateAttributeGroupAsync(
            name: "Specifications",
            attributeSetId: attributeSet.AttributeSetId,
            sortOrder: 1
        );

        // Create attributes for this group
        await _attributeService.CreateAttributeAsync(
            attributeCode: "screen_size",
            backendType: "decimal",
            frontendInput: "text",
            frontendLabel: "Screen Size (inches)",
            attributeGroupId: specsGroup.AttributeGroupId
        );
    }

    // Get all attributes
    public async Task<IEnumerable<object>> GetAllAttributes()
    {
        return await _attributeService.GetAllAttributesAsync();
    }
}
```

### Working with Inventory

```csharp
public class InventoryExamples
{
    private readonly IProductService _productService;

    public InventoryExamples(IProductService productService)
    {
        _productService = productService;
    }

    // Update inventory
    public async Task UpdateInventory(Guid productId, int qty, bool isInStock)
    {
        await _productService.UpdateInventoryAsync(productId, qty, isInStock);
    }

    // Get inventory
    public async Task<object?> GetInventory(Guid productId)
    {
        return await _productService.GetInventoryAsync(productId);
    }
}
```

### Multi-Store Support

```csharp
public class MultiStoreExamples
{
    private readonly IProductService _productService;

    public MultiStoreExamples(IProductService productService)
    {
        _productService = productService;
    }

    // Set store-specific attribute value
    public async Task SetStoreSpecificValue(Guid productId)
    {
        // Set global value (storeId = 0)
        await _productService.SetAttributeAsync(productId, "name", "Blue Shirt", storeId: 0);

        // Set French store value (storeId = 1)
        await _productService.SetAttributeAsync(productId, "name", "Chemise Bleue", storeId: 1);

        // Set German store value (storeId = 2)
        await _productService.SetAttributeAsync(productId, "name", "Blaues Hemd", storeId: 2);
    }

    // Get store-specific value (with global fallback)
    public async Task<string?> GetStoreSpecificValue(Guid productId, int storeId)
    {
        // If store-specific value exists, returns it
        // Otherwise falls back to global value (storeId = 0)
        return await _productService.GetAttributeAsync<string>(productId, "name", storeId);
    }
}
```

---

## API Reference

**Note:** All ID parameters (productId, categoryId, attributeId, attributeSetId, attributeGroupId, etc.) are of type `Guid` (UUID), not `int`.

### IProductService

| Method | Description |
|--------|-------------|
| `GetProductAsync(id, storeId)` | Get product by ID (id: Guid) |
| `GetProductBySkuAsync(sku, storeId)` | Get product by SKU |
| `GetProductsAsync(storeId, page, pageSize)` | Get paginated products |
| `CreateProductAsync(sku, typeId, attributeSetId)` | Create new product (attributeSetId: Guid) |
| `UpdateProductAsync(product)` | Update product |
| `DeleteProductAsync(id)` | Delete product (id: Guid) |
| `GetAttributeAsync<T>(productId, attributeCode, storeId)` | Get attribute value (productId: Guid) |
| `SetAttributeAsync(productId, attributeCode, value, storeId)` | Set attribute value (productId: Guid) |
| `SetAttributesAsync(productId, attributes, storeId)` | Set multiple attributes (productId: Guid) |
| `GetProductsByCategoryAsync(categoryId, storeId, page, pageSize)` | Get products by category (categoryId: Guid) |
| `AssignProductToCategoryAsync(productId, categoryId, position)` | Assign to category (both IDs: Guid) |
| `RemoveProductFromCategoryAsync(productId, categoryId)` | Remove from category (both IDs: Guid) |
| `UpdateInventoryAsync(productId, qty, isInStock)` | Update inventory (productId: Guid) |
| `GetInventoryAsync(productId)` | Get inventory (productId: Guid) |
| `SearchProductsAsync(query, storeId, page, pageSize)` | Search products |

### ICategoryService

| Method | Description |
|--------|-------------|
| `GetCategoryAsync(id)` | Get category by ID (id: Guid) |
| `GetAllCategoriesAsync()` | Get all categories |
| `GetRootCategoriesAsync()` | Get root categories |
| `GetCategoryTreeAsync(rootId)` | Get category tree (rootId: Guid?) |
| `CreateCategoryAsync(name, parentId)` | Create category (parentId: Guid?) |
| `UpdateCategoryAsync(category)` | Update category |
| `DeleteCategoryAsync(id)` | Delete category (id: Guid) |
| `MoveCategoryAsync(categoryId, newParentId, position)` | Move category (both IDs: Guid) |

### IAttributeService

| Method | Description |
|--------|-------------|
| `GetAttributeAsync(id)` | Get attribute by ID (id: Guid) |
| `GetAttributeByCodeAsync(code)` | Get attribute by code |
| `GetAllAttributesAsync()` | Get all attributes |
| `CreateAttributeAsync(...)` | Create attribute (attributeGroupId: Guid?) |
| `UpdateAttributeAsync(attribute)` | Update attribute |
| `DeleteAttributeAsync(id)` | Delete attribute (id: Guid) |
| `GetAttributeSetAsync(id)` | Get attribute set (id: Guid) |
| `GetAllAttributeSetsAsync()` | Get all attribute sets |
| `CreateAttributeSetAsync(name)` | Create attribute set |
| `GetAttributeGroupAsync(id)` | Get attribute group (id: Guid) |
| `GetAttributeGroupsBySetAsync(setId)` | Get groups by set (setId: Guid) |
| `CreateAttributeGroupAsync(name, setId, sortOrder)` | Create attribute group (setId: Guid) |

---

## Extending the Library

### Custom Product Repository

```csharp
public class CustomProductRepository : ProductRepository
{
    public CustomProductRepository(ProductCatalogDbContext context, IMemoryCache cache)
        : base(context, cache)
    {
    }

    // Add custom methods
    public async Task<IEnumerable<ProductEntity>> GetFeaturedProductsAsync()
    {
        // Custom implementation
    }

    // Override existing methods
    public override async Task<ProductEntity> CreateAsync(ProductEntity product, CancellationToken ct = default)
    {
        // Add custom logic before creation
        product.CreatedAt = DateTime.UtcNow;
        
        return await base.CreateAsync(product, ct);
    }
}

// Register in Program.cs
builder.Services.AddProductCatalog<CustomProductRepository>(builder.Configuration);
```

### Custom Product Service

```csharp
public class CustomProductService : ProductService
{
    private readonly ILogger<CustomProductService> _logger;

    public CustomProductService(
        IProductRepository repository,
        ProductCatalogDbContext context,
        ILogger<CustomProductService> logger)
        : base(repository, context)
    {
        _logger = logger;
    }

    public override async Task<ProductEntity> CreateProductAsync(
        string sku, string typeId = "simple", Guid attributeSetId = default, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating product with SKU: {Sku}", sku);
        
        var product = await base.CreateProductAsync(sku, typeId, attributeSetId, ct);
        
        // Add custom logic after creation
        _logger.LogInformation("Product created with ID: {Id}", product.EntityId);
        
        return product;
    }
}

// Register in Program.cs
builder.Services.AddProductCatalogWithCustomServices<
    CustomProductService,
    CategoryService,
    AttributeService>(builder.Configuration);
```

---

## Database Schema

### Core Tables

| Table | Description |
|-------|-------------|
| `ProductEntity` | Main product table (SKU, type, attribute set) |
| `EavAttribute` | Attribute definitions |
| `EavAttributeSet` | Attribute sets (e.g., Default, Electronics) |
| `EavAttributeGroup` | Groups within sets (e.g., General, Prices) |

### EAV Value Tables

| Table | Data Type | Example Attributes |
|-------|-----------|-------------------|
| `ProductEntityVarchar` | String (max 255) | name, url_key, manufacturer |
| `ProductEntityInt` | Integer | status, visibility, color_id |
| `ProductEntityDecimal` | Decimal | price, weight, special_price |
| `ProductEntityText` | Long text | description, short_description |
| `ProductEntityDatetime` | DateTime | special_from_date, news_from_date |

### Other Tables

| Table | Description |
|-------|-------------|
| `CategoryEntity` | Product categories |
| `ProductCategory` | Product-category relations |
| `ProductInventory` | Stock information |
| `ProductPrice` | Tier pricing |
| `ProductMediaGallery` | Images and videos |
| `ProductRelation` | Product relationships |
| `ProductWebsite` | Multi-website assignments |

**Note:** All primary keys use `Guid` (UUID) type. Table names use PascalCase convention.

---

## Default Seed Attributes

The library seeds these default attributes:

| Code | Type | Label |
|------|------|-------|
| `name` | varchar | Product Name |
| `description` | text | Description |
| `short_description` | text | Short Description |
| `price` | decimal | Price |
| `special_price` | decimal | Special Price |
| `status` | int | Status |
| `visibility` | int | Visibility |
| `weight` | decimal | Weight |
| `url_key` | varchar | URL Key |
| `meta_title` | varchar | Meta Title |
| `meta_description` | text | Meta Description |
| `meta_keywords` | text | Meta Keywords |
| `image` | varchar | Base Image |
| `small_image` | varchar | Small Image |
| `thumbnail` | varchar | Thumbnail |
| `color` | int | Color |
| `size` | int | Size |
| `manufacturer` | varchar | Manufacturer |
| `special_from_date` | datetime | Special Price From |
| `special_to_date` | datetime | Special Price To |

---

## Best Practices

1. **Use Attribute Codes Consistently** - Define attribute codes once and reuse them
2. **Leverage Caching** - Attribute metadata is cached for 30 minutes by default
3. **Use Store IDs for Multi-Language** - Store ID 0 = global, >0 = store-specific
4. **Batch Attribute Updates** - Use `SetAttributesAsync()` instead of multiple `SetAttributeAsync()` calls
5. **Create Attribute Sets for Product Types** - Different product types may need different attributes
6. **Index Frequently Filtered Attributes** - Mark attributes as `IsFilterable` for better query performance

---

## License

MIT License - See LICENSE file for details.
