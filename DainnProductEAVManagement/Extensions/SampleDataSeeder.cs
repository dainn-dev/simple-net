using DainnProductEAVManagement.Contexts;
using DainnProductEAVManagement.Entities;
using DainnProductEAVManagement.ValueEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DainnProductEAVManagement.Extensions;

/// <summary>
/// Provides methods to seed sample data for testing and demonstration purposes.
/// </summary>
public static class SampleDataSeeder
{
    /// <summary>
    /// Seeds the database with sample products, categories, and attribute values.
    /// This is useful for testing and demonstration purposes.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public static void SeedSampleData(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ProductCatalogDbContext>>();

        try
        {
            // Check if sample data already exists
            if (context.Products.Any())
            {
                logger?.LogInformation("Sample data already exists. Skipping sample data seeding.");
                return;
            }

            logger?.LogInformation("Seeding sample data for ProductCatalog...");

            // Seed categories
            SeedCategories(context, logger);

            // Seed products with attributes
            SeedProducts(context, logger);

            logger?.LogInformation("Sample data seeded successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding sample data: {Error}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Async version of SeedSampleData.
    /// </summary>
    public static async Task SeedSampleDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ProductCatalogDbContext>>();

        try
        {
            // Check if sample data already exists
            if (await context.Products.AnyAsync())
            {
                logger?.LogInformation("Sample data already exists. Skipping sample data seeding.");
                return;
            }

            logger?.LogInformation("Seeding sample data for ProductCatalog...");

            // Seed categories
            await SeedCategoriesAsync(context, logger);

            // Seed products with attributes
            await SeedProductsAsync(context, logger);

            logger?.LogInformation("Sample data seeded successfully.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding sample data: {Error}", ex.Message);
            throw;
        }
    }

    private static void SeedCategories(ProductCatalogDbContext context, ILogger? logger)
    {
        var now = DateTime.UtcNow;

        // Root categories
        var electronics = new CategoryEntity
        {
            Name = "Electronics",
            UrlKey = "electronics",
            Description = "Electronic devices and gadgets",
            IsActive = true,
            Level = 1,
            Position = 1,
            Path = "1",
            CreatedAt = now,
            UpdatedAt = now
        };

        var clothing = new CategoryEntity
        {
            Name = "Clothing",
            UrlKey = "clothing",
            Description = "Fashion and apparel for all occasions",
            IsActive = true,
            Level = 1,
            Position = 2,
            Path = "2",
            CreatedAt = now,
            UpdatedAt = now
        };

        var homeGarden = new CategoryEntity
        {
            Name = "Home & Garden",
            UrlKey = "home-garden",
            Description = "Products for your home and garden",
            IsActive = true,
            Level = 1,
            Position = 3,
            Path = "3",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Categories.AddRange(electronics, clothing, homeGarden);
        context.SaveChanges();

        // Update paths after IDs are generated
        electronics.Path = electronics.EntityId.ToString("N");
        clothing.Path = clothing.EntityId.ToString("N");
        homeGarden.Path = homeGarden.EntityId.ToString("N");
        context.SaveChanges();

        // Sub-categories for Electronics
        var computers = new CategoryEntity
        {
            Name = "Computers & Laptops",
            UrlKey = "computers-laptops",
            Description = "Desktop computers, laptops, and accessories",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var phones = new CategoryEntity
        {
            Name = "Smartphones & Tablets",
            UrlKey = "smartphones-tablets",
            Description = "Mobile phones, tablets, and accessories",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var audio = new CategoryEntity
        {
            Name = "Audio & Headphones",
            UrlKey = "audio-headphones",
            Description = "Speakers, headphones, and audio equipment",
            IsActive = true,
            Level = 2,
            Position = 3,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Sub-categories for Clothing
        var mensClothing = new CategoryEntity
        {
            Name = "Men's Clothing",
            UrlKey = "mens-clothing",
            Description = "Clothing and fashion for men",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = clothing.EntityId,
            Path = $"{clothing.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var womensClothing = new CategoryEntity
        {
            Name = "Women's Clothing",
            UrlKey = "womens-clothing",
            Description = "Clothing and fashion for women",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = clothing.EntityId,
            Path = $"{clothing.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Sub-categories for Home & Garden
        var furniture = new CategoryEntity
        {
            Name = "Furniture",
            UrlKey = "furniture",
            Description = "Home and office furniture",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = homeGarden.EntityId,
            Path = $"{homeGarden.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var decor = new CategoryEntity
        {
            Name = "Home Decor",
            UrlKey = "home-decor",
            Description = "Decorations and accessories for your home",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = homeGarden.EntityId,
            Path = $"{homeGarden.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Categories.AddRange(computers, phones, audio, mensClothing, womensClothing, furniture, decor);
        context.SaveChanges();

        // Update paths after IDs are generated
        computers.Path = $"{electronics.EntityId}/{computers.EntityId}";
        phones.Path = $"{electronics.EntityId}/{phones.EntityId}";
        audio.Path = $"{electronics.EntityId}/{audio.EntityId}";
        mensClothing.Path = $"{clothing.EntityId}/{mensClothing.EntityId}";
        womensClothing.Path = $"{clothing.EntityId}/{womensClothing.EntityId}";
        furniture.Path = $"{homeGarden.EntityId}/{furniture.EntityId}";
        decor.Path = $"{homeGarden.EntityId}/{decor.EntityId}";
        context.SaveChanges();

        logger?.LogInformation("Seeded {Count} categories.", context.Categories.Count());
    }

    private static async Task SeedCategoriesAsync(ProductCatalogDbContext context, ILogger? logger)
    {
        var now = DateTime.UtcNow;

        // Root categories
        var electronics = new CategoryEntity
        {
            Name = "Electronics",
            UrlKey = "electronics",
            Description = "Electronic devices and gadgets",
            IsActive = true,
            Level = 1,
            Position = 1,
            Path = "1",
            CreatedAt = now,
            UpdatedAt = now
        };

        var clothing = new CategoryEntity
        {
            Name = "Clothing",
            UrlKey = "clothing",
            Description = "Fashion and apparel for all occasions",
            IsActive = true,
            Level = 1,
            Position = 2,
            Path = "2",
            CreatedAt = now,
            UpdatedAt = now
        };

        var homeGarden = new CategoryEntity
        {
            Name = "Home & Garden",
            UrlKey = "home-garden",
            Description = "Products for your home and garden",
            IsActive = true,
            Level = 1,
            Position = 3,
            Path = "3",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Categories.AddRange(electronics, clothing, homeGarden);
        await context.SaveChangesAsync();

        // Update paths
        electronics.Path = electronics.EntityId.ToString();
        clothing.Path = clothing.EntityId.ToString();
        homeGarden.Path = homeGarden.EntityId.ToString();
        await context.SaveChangesAsync();

        // Sub-categories for Electronics
        var computers = new CategoryEntity
        {
            Name = "Computers & Laptops",
            UrlKey = "computers-laptops",
            Description = "Desktop computers, laptops, and accessories",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var phones = new CategoryEntity
        {
            Name = "Smartphones & Tablets",
            UrlKey = "smartphones-tablets",
            Description = "Mobile phones, tablets, and accessories",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var audio = new CategoryEntity
        {
            Name = "Audio & Headphones",
            UrlKey = "audio-headphones",
            Description = "Speakers, headphones, and audio equipment",
            IsActive = true,
            Level = 2,
            Position = 3,
            ParentId = electronics.EntityId,
            Path = $"{electronics.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Sub-categories for Clothing
        var mensClothing = new CategoryEntity
        {
            Name = "Men's Clothing",
            UrlKey = "mens-clothing",
            Description = "Clothing and fashion for men",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = clothing.EntityId,
            Path = $"{clothing.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var womensClothing = new CategoryEntity
        {
            Name = "Women's Clothing",
            UrlKey = "womens-clothing",
            Description = "Clothing and fashion for women",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = clothing.EntityId,
            Path = $"{clothing.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Sub-categories for Home & Garden
        var furniture = new CategoryEntity
        {
            Name = "Furniture",
            UrlKey = "furniture",
            Description = "Home and office furniture",
            IsActive = true,
            Level = 2,
            Position = 1,
            ParentId = homeGarden.EntityId,
            Path = $"{homeGarden.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        var decor = new CategoryEntity
        {
            Name = "Home Decor",
            UrlKey = "home-decor",
            Description = "Decorations and accessories for your home",
            IsActive = true,
            Level = 2,
            Position = 2,
            ParentId = homeGarden.EntityId,
            Path = $"{homeGarden.EntityId:N}",
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Categories.AddRange(computers, phones, audio, mensClothing, womensClothing, furniture, decor);
        await context.SaveChangesAsync();

        // Update paths
        computers.Path = $"{electronics.EntityId}/{computers.EntityId}";
        phones.Path = $"{electronics.EntityId}/{phones.EntityId}";
        audio.Path = $"{electronics.EntityId}/{audio.EntityId}";
        mensClothing.Path = $"{clothing.EntityId}/{mensClothing.EntityId}";
        womensClothing.Path = $"{clothing.EntityId}/{womensClothing.EntityId}";
        furniture.Path = $"{homeGarden.EntityId}/{furniture.EntityId}";
        decor.Path = $"{homeGarden.EntityId}/{decor.EntityId}";
        await context.SaveChangesAsync();

        logger?.LogInformation("Seeded {Count} categories.", await context.Categories.CountAsync());
    }

    private static void SeedProducts(ProductCatalogDbContext context, ILogger? logger)
    {
        var now = DateTime.UtcNow;
        var categories = context.Categories.ToList();
        var attributes = context.Attributes.ToList();

        // Get default attribute set ID
        var defaultAttributeSet = context.AttributeSets.FirstOrDefault(a => a.AttributeSetName == "Default");
        if (defaultAttributeSet == null)
        {
            logger?.LogWarning("Default attribute set not found. Cannot seed products.");
            return;
        }
        var defaultAttributeSetId = defaultAttributeSet.AttributeSetId;

        // ========== ELECTRONICS PRODUCTS ==========
        var laptop = CreateProduct(context, "LAPTOP-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, laptop.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "ProBook Laptop 15.6\"",
            ["description"] = "High-performance laptop with 15.6\" Full HD display, Intel Core i7 processor, 16GB RAM, and 512GB SSD. Perfect for work and entertainment.",
            ["short_description"] = "Powerful 15.6\" laptop with Intel i7, 16GB RAM, 512GB SSD",
            ["price"] = 1299.99m,
            ["special_price"] = 1199.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 2.1m,
            ["url_key"] = "probook-laptop-15-6",
            ["meta_title"] = "ProBook Laptop 15.6\" - High Performance Laptop",
            ["meta_description"] = "Buy ProBook Laptop 15.6\" with Intel i7, 16GB RAM, 512GB SSD. Fast shipping and great prices.",
            ["manufacturer"] = "TechCorp"
        }, attributes);
        AddInventory(context, laptop.EntityId, 50, true);
        AssignToCategory(context, laptop.EntityId, categories.First(c => c.UrlKey == "computers-laptops").EntityId);

        var smartphone = CreateProduct(context, "PHONE-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, smartphone.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Galaxy Ultra Pro 5G",
            ["description"] = "Flagship smartphone with 6.7\" Dynamic AMOLED display, 108MP camera, 5000mAh battery, and 5G connectivity. The ultimate mobile experience.",
            ["short_description"] = "Flagship 5G smartphone with 108MP camera",
            ["price"] = 999.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.22m,
            ["url_key"] = "galaxy-ultra-pro-5g",
            ["meta_title"] = "Galaxy Ultra Pro 5G - Flagship Smartphone",
            ["manufacturer"] = "TechCorp"
        }, attributes);
        AddInventory(context, smartphone.EntityId, 100, true);
        AssignToCategory(context, smartphone.EntityId, categories.First(c => c.UrlKey == "smartphones-tablets").EntityId);

        var tablet = CreateProduct(context, "TABLET-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, tablet.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "iPad Pro 12.9\"",
            ["description"] = "Professional tablet with M2 chip, 12.9\" Liquid Retina XDR display, and all-day battery life. Perfect for creatives and professionals.",
            ["short_description"] = "Professional tablet with M2 chip and 12.9\" display",
            ["price"] = 1099.00m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.68m,
            ["url_key"] = "ipad-pro-12-9",
            ["manufacturer"] = "AppleTech"
        }, attributes);
        AddInventory(context, tablet.EntityId, 75, true);
        AssignToCategory(context, tablet.EntityId, categories.First(c => c.UrlKey == "smartphones-tablets").EntityId);

        var headphones = CreateProduct(context, "AUDIO-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, headphones.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "NoiseCancel Pro Headphones",
            ["description"] = "Premium wireless headphones with active noise cancellation, 30-hour battery life, and Hi-Res audio support.",
            ["short_description"] = "Premium ANC wireless headphones",
            ["price"] = 349.99m,
            ["special_price"] = 299.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.25m,
            ["url_key"] = "noisecancel-pro-headphones",
            ["manufacturer"] = "AudioMax"
        }, attributes);
        AddInventory(context, headphones.EntityId, 200, true);
        AssignToCategory(context, headphones.EntityId, categories.First(c => c.UrlKey == "audio-headphones").EntityId);

        var speaker = CreateProduct(context, "AUDIO-002", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, speaker.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "BoomBox Portable Speaker",
            ["description"] = "Powerful portable Bluetooth speaker with 360° sound, waterproof design (IPX7), and 20-hour battery life.",
            ["short_description"] = "Waterproof portable Bluetooth speaker",
            ["price"] = 199.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.95m,
            ["url_key"] = "boombox-portable-speaker",
            ["manufacturer"] = "AudioMax"
        }, attributes);
        AddInventory(context, speaker.EntityId, 150, true);
        AssignToCategory(context, speaker.EntityId, categories.First(c => c.UrlKey == "audio-headphones").EntityId);

        // ========== CLOTHING PRODUCTS ==========
        var mensShirt = CreateProduct(context, "SHIRT-M-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, mensShirt.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Classic Oxford Button-Down Shirt",
            ["description"] = "Timeless Oxford button-down shirt made from 100% premium cotton. Perfect for both casual and formal occasions.",
            ["short_description"] = "Classic 100% cotton Oxford shirt",
            ["price"] = 79.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.3m,
            ["url_key"] = "classic-oxford-button-down-shirt",
            ["color"] = 1,
            ["size"] = 3,
            ["manufacturer"] = "StyleCo"
        }, attributes);
        AddInventory(context, mensShirt.EntityId, 300, true);
        AssignToCategory(context, mensShirt.EntityId, categories.First(c => c.UrlKey == "mens-clothing").EntityId);

        var mensPants = CreateProduct(context, "PANTS-M-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, mensPants.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Slim Fit Chino Pants",
            ["description"] = "Modern slim-fit chino pants with stretch fabric for maximum comfort. Features classic design with functional pockets.",
            ["short_description"] = "Comfortable slim-fit chinos with stretch",
            ["price"] = 89.99m,
            ["special_price"] = 69.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.5m,
            ["url_key"] = "slim-fit-chino-pants",
            ["color"] = 2,
            ["size"] = 4,
            ["manufacturer"] = "StyleCo"
        }, attributes);
        AddInventory(context, mensPants.EntityId, 250, true);
        AssignToCategory(context, mensPants.EntityId, categories.First(c => c.UrlKey == "mens-clothing").EntityId);

        var womensDress = CreateProduct(context, "DRESS-W-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, womensDress.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Elegant Summer Maxi Dress",
            ["description"] = "Beautiful flowy maxi dress perfect for summer occasions. Made from breathable fabric with a flattering A-line silhouette.",
            ["short_description"] = "Elegant A-line maxi dress for summer",
            ["price"] = 129.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.4m,
            ["url_key"] = "elegant-summer-maxi-dress",
            ["color"] = 3,
            ["size"] = 2,
            ["manufacturer"] = "Fashionista"
        }, attributes);
        AddInventory(context, womensDress.EntityId, 180, true);
        AssignToCategory(context, womensDress.EntityId, categories.First(c => c.UrlKey == "womens-clothing").EntityId);

        var womensBlouse = CreateProduct(context, "BLOUSE-W-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, womensBlouse.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Silk Blend Blouse",
            ["description"] = "Luxurious silk-blend blouse with delicate details. Perfect for the office or evening events.",
            ["short_description"] = "Luxurious silk-blend blouse",
            ["price"] = 99.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.2m,
            ["url_key"] = "silk-blend-blouse",
            ["color"] = 4,
            ["size"] = 2,
            ["manufacturer"] = "Fashionista"
        }, attributes);
        AddInventory(context, womensBlouse.EntityId, 150, true);
        AssignToCategory(context, womensBlouse.EntityId, categories.First(c => c.UrlKey == "womens-clothing").EntityId);

        // ========== HOME & GARDEN PRODUCTS ==========
        var sofa = CreateProduct(context, "SOFA-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, sofa.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Modern 3-Seater Sofa",
            ["description"] = "Contemporary 3-seater sofa with premium fabric upholstery and solid wood legs. Comfortable and stylish addition to any living room.",
            ["short_description"] = "Contemporary 3-seater fabric sofa",
            ["price"] = 899.99m,
            ["special_price"] = 799.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 45.0m,
            ["url_key"] = "modern-3-seater-sofa",
            ["color"] = 5,
            ["manufacturer"] = "HomeStyle"
        }, attributes);
        AddInventory(context, sofa.EntityId, 25, true);
        AssignToCategory(context, sofa.EntityId, categories.First(c => c.UrlKey == "furniture").EntityId);

        var diningTable = CreateProduct(context, "TABLE-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, diningTable.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Solid Oak Dining Table",
            ["description"] = "Beautiful solid oak dining table that seats 6-8 people. Crafted from sustainably sourced oak with a natural finish.",
            ["short_description"] = "Solid oak dining table for 6-8 people",
            ["price"] = 1299.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 80.0m,
            ["url_key"] = "solid-oak-dining-table",
            ["manufacturer"] = "HomeStyle"
        }, attributes);
        AddInventory(context, diningTable.EntityId, 15, true);
        AssignToCategory(context, diningTable.EntityId, categories.First(c => c.UrlKey == "furniture").EntityId);

        var lamp = CreateProduct(context, "LAMP-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, lamp.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Scandinavian Floor Lamp",
            ["description"] = "Minimalist Scandinavian-style floor lamp with adjustable head. Features warm LED lighting and wooden tripod base.",
            ["short_description"] = "Minimalist Scandinavian floor lamp",
            ["price"] = 149.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 3.5m,
            ["url_key"] = "scandinavian-floor-lamp",
            ["manufacturer"] = "LightCo"
        }, attributes);
        AddInventory(context, lamp.EntityId, 80, true);
        AssignToCategory(context, lamp.EntityId, categories.First(c => c.UrlKey == "home-decor").EntityId);

        var vase = CreateProduct(context, "VASE-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, vase.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Ceramic Artisan Vase",
            ["description"] = "Handcrafted ceramic vase with unique glazed finish. Each piece is one-of-a-kind, made by skilled artisans.",
            ["short_description"] = "Handcrafted ceramic vase",
            ["price"] = 59.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 1.2m,
            ["url_key"] = "ceramic-artisan-vase",
            ["color"] = 6,
            ["manufacturer"] = "ArtisanHome"
        }, attributes);
        AddInventory(context, vase.EntityId, 120, true);
        AssignToCategory(context, vase.EntityId, categories.First(c => c.UrlKey == "home-decor").EntityId);

        var cushion = CreateProduct(context, "CUSHION-001", "simple", defaultAttributeSetId, now);
        SetProductAttributes(context, cushion.EntityId, new Dictionary<string, object?>
        {
            ["name"] = "Velvet Throw Cushion Set",
            ["description"] = "Set of 2 luxurious velvet throw cushions. Features soft velvet cover with invisible zipper and hypoallergenic filling.",
            ["short_description"] = "Set of 2 velvet throw cushions",
            ["price"] = 49.99m,
            ["special_price"] = 39.99m,
            ["status"] = 1,
            ["visibility"] = 4,
            ["weight"] = 0.8m,
            ["url_key"] = "velvet-throw-cushion-set",
            ["color"] = 7,
            ["manufacturer"] = "HomeStyle"
        }, attributes);
        AddInventory(context, cushion.EntityId, 200, true);
        AssignToCategory(context, cushion.EntityId, categories.First(c => c.UrlKey == "home-decor").EntityId);

        context.SaveChanges();
        logger?.LogInformation("Seeded {Count} products with attributes, inventory, and category assignments.", context.Products.Count());
    }

    private static async Task SeedProductsAsync(ProductCatalogDbContext context, ILogger? logger)
    {
        // For simplicity, we'll reuse the sync method wrapped in Task.Run
        await Task.Run(() => SeedProducts(context, logger));
    }

    private static ProductEntity CreateProduct(ProductCatalogDbContext context, string sku, string typeId, Guid attributeSetId, DateTime now)
    {
        var product = new ProductEntity
        {
            Sku = sku,
            TypeId = typeId,
            AttributeSetId = attributeSetId,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }

    private static void SetProductAttributes(ProductCatalogDbContext context, Guid entityId, Dictionary<string, object?> values, List<EavAttribute> attributes)
    {
        foreach (var kvp in values)
        {
            var attr = attributes.FirstOrDefault(a => a.AttributeCode == kvp.Key);
            if (attr == null || kvp.Value == null) continue;

            switch (attr.BackendType.ToLowerInvariant())
            {
                case "varchar":
                    context.ProductVarcharValues.Add(new ProductVarcharValue
                    {
                        EntityId = entityId,
                        AttributeId = attr.AttributeId,
                        StoreId = 0,
                        Value = kvp.Value.ToString()
                    });
                    break;

                case "text":
                    context.ProductTextValues.Add(new ProductTextValue
                    {
                        EntityId = entityId,
                        AttributeId = attr.AttributeId,
                        StoreId = 0,
                        Value = kvp.Value.ToString()
                    });
                    break;

                case "int":
                    context.ProductIntValues.Add(new ProductIntValue
                    {
                        EntityId = entityId,
                        AttributeId = attr.AttributeId,
                        StoreId = 0,
                        Value = Convert.ToInt32(kvp.Value)
                    });
                    break;

                case "decimal":
                    context.ProductDecimalValues.Add(new ProductDecimalValue
                    {
                        EntityId = entityId,
                        AttributeId = attr.AttributeId,
                        StoreId = 0,
                        Value = Convert.ToDecimal(kvp.Value)
                    });
                    break;

                case "datetime":
                    context.ProductDatetimeValues.Add(new ProductDatetimeValue
                    {
                        EntityId = entityId,
                        AttributeId = attr.AttributeId,
                        StoreId = 0,
                        Value = Convert.ToDateTime(kvp.Value)
                    });
                    break;
            }
        }
        context.SaveChanges();
    }

    private static void AddInventory(ProductCatalogDbContext context, Guid productId, int qty, bool isInStock)
    {
        context.ProductInventories.Add(new ProductInventory
        {
            ProductId = productId,
            Qty = qty,
            IsInStock = isInStock,
            MinQty = 0,
            MinSaleQty = 1,
            MaxSaleQty = 10000,
            ManageStock = true,
            BackOrders = false,
            NotifyStockQty = 10,
            QtyIncrement = 1
        });
    }

    private static void AssignToCategory(ProductCatalogDbContext context, Guid productId, Guid categoryId)
    {
        context.ProductCategoryRelations.Add(new ProductCategoryRelation
        {
            ProductId = productId,
            CategoryId = categoryId,
            Position = 0
        });
    }
}
