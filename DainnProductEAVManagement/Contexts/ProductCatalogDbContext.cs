using DainnProductEAVManagement.Entities;
using DainnProductEAVManagement.ValueEntities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DainnProductEAVManagement.Contexts;

/// <summary>
/// DbContext for the Product Catalog EAV system.
/// Implements EAV pattern with separate tables for each value type.
/// </summary>
public class ProductCatalogDbContext : DbContext
{
    public ProductCatalogDbContext(DbContextOptions<ProductCatalogDbContext> options)
        : base(options)
    {
    }

    // Core entities
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<EavAttribute> Attributes { get; set; } = null!;
    public DbSet<AttributeSet> AttributeSets { get; set; } = null!;
    public DbSet<AttributeGroup> AttributeGroups { get; set; } = null!;

    // EAV Value tables
    public DbSet<ProductVarcharValue> ProductVarcharValues { get; set; } = null!;
    public DbSet<ProductIntValue> ProductIntValues { get; set; } = null!;
    public DbSet<ProductDecimalValue> ProductDecimalValues { get; set; } = null!;
    public DbSet<ProductTextValue> ProductTextValues { get; set; } = null!;
    public DbSet<ProductDatetimeValue> ProductDatetimeValues { get; set; } = null!;

    // Other entities
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<ProductCategoryRelation> ProductCategoryRelations { get; set; } = null!;
    public DbSet<ProductInventory> ProductInventories { get; set; } = null!;
    public DbSet<ProductPrice> ProductPrices { get; set; } = null!;
    public DbSet<ProductMediaGallery> ProductMediaGalleries { get; set; } = null!;
    public DbSet<ProductRelation> ProductRelations { get; set; } = null!;
    public DbSet<ProductWebsite> ProductWebsites { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all Guid properties to use UUID type in PostgreSQL
        if (Database.IsNpgsql())
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(Guid) || p.ClrType == typeof(Guid?)))
                {
                    property.SetColumnType("uuid");
                }
            }
        }

        // ========== ProductEntity Configuration ==========
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("ProductEntity");
            entity.HasKey(e => e.EntityId);
            entity.Property(e => e.EntityId).HasColumnName("entity_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Sku).HasColumnName("sku").HasMaxLength(64).IsRequired();
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.TypeId).HasColumnName("type_id").HasMaxLength(32).HasDefaultValue("simple");
            entity.Property(e => e.AttributeSetId).HasColumnName("attribute_set_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.AttributeSet)
                .WithMany(a => a.Products)
                .HasForeignKey(e => e.AttributeSetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== AttributeSet Configuration ==========
        modelBuilder.Entity<AttributeSet>(entity =>
        {
            entity.ToTable("EavAttributeSet");
            entity.HasKey(e => e.AttributeSetId);
            entity.Property(e => e.AttributeSetId).HasColumnName("attribute_set_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityTypeId).HasColumnName("entity_type_id").HasDefaultValue(4);
            entity.Property(e => e.AttributeSetName).HasColumnName("attribute_set_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        // ========== AttributeGroup Configuration ==========
        modelBuilder.Entity<AttributeGroup>(entity =>
        {
            entity.ToTable("EavAttributeGroup");
            entity.HasKey(e => e.AttributeGroupId);
            entity.Property(e => e.AttributeGroupId).HasColumnName("attribute_group_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AttributeSetId).HasColumnName("attribute_set_id");
            entity.Property(e => e.AttributeGroupName).HasColumnName("attribute_group_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(e => e.AttributeSet)
                .WithMany(a => a.Groups)
                .HasForeignKey(e => e.AttributeSetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== EavAttribute Configuration ==========
        modelBuilder.Entity<EavAttribute>(entity =>
        {
            entity.ToTable("EavAttribute");
            entity.HasKey(e => e.AttributeId);
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityTypeId).HasColumnName("entity_type_id").HasDefaultValue(4);
            entity.Property(e => e.AttributeCode).HasColumnName("attribute_code").HasMaxLength(255).IsRequired();
            entity.HasIndex(e => new { e.EntityTypeId, e.AttributeCode }).IsUnique();
            entity.Property(e => e.BackendType).HasColumnName("backend_type").HasMaxLength(8).IsRequired();
            entity.Property(e => e.FrontendInput).HasColumnName("frontend_input").HasMaxLength(50);
            entity.Property(e => e.FrontendLabel).HasColumnName("frontend_label").HasMaxLength(255);
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.IsUnique).HasColumnName("is_unique");
            entity.Property(e => e.IsSearchable).HasColumnName("is_searchable");
            entity.Property(e => e.IsFilterable).HasColumnName("is_filterable");
            entity.Property(e => e.IsComparable).HasColumnName("is_comparable");
            entity.Property(e => e.IsVisibleOnFront).HasColumnName("is_visible_on_front");
            entity.Property(e => e.IsHtmlAllowedOnFront).HasColumnName("is_html_allowed_on_front");
            entity.Property(e => e.IsUsedForPromoRules).HasColumnName("is_used_for_promo_rules");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.DefaultValue).HasColumnName("default_value").HasMaxLength(255);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255);
            entity.Property(e => e.AttributeGroupId).HasColumnName("attribute_group_id");

            entity.HasOne(e => e.Group)
                .WithMany(g => g.Attributes)
                .HasForeignKey(e => e.AttributeGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ========== ProductVarcharValue Configuration ==========
        modelBuilder.Entity<ProductVarcharValue>(entity =>
        {
            entity.ToTable("ProductEntityVarchar");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Value).HasColumnName("value").HasMaxLength(255);
            entity.HasIndex(e => new { e.EntityId, e.AttributeId, e.StoreId }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.VarcharValues)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany(a => a.VarcharValues)
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductIntValue Configuration ==========
        modelBuilder.Entity<ProductIntValue>(entity =>
        {
            entity.ToTable("ProductEntityInt");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Value).HasColumnName("value");
            entity.HasIndex(e => new { e.EntityId, e.AttributeId, e.StoreId }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.IntValues)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany(a => a.IntValues)
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductDecimalValue Configuration ==========
        modelBuilder.Entity<ProductDecimalValue>(entity =>
        {
            entity.ToTable("ProductEntityDecimal");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Value).HasColumnName("value").HasPrecision(20, 6);
            entity.HasIndex(e => new { e.EntityId, e.AttributeId, e.StoreId }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.DecimalValues)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany(a => a.DecimalValues)
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductTextValue Configuration ==========
        modelBuilder.Entity<ProductTextValue>(entity =>
        {
            entity.ToTable("ProductEntityText");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Value).HasColumnName("value");
            entity.HasIndex(e => new { e.EntityId, e.AttributeId, e.StoreId }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.TextValues)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany(a => a.TextValues)
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductDatetimeValue Configuration ==========
        modelBuilder.Entity<ProductDatetimeValue>(entity =>
        {
            entity.ToTable("ProductEntityDatetime");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Value).HasColumnName("value");
            entity.HasIndex(e => new { e.EntityId, e.AttributeId, e.StoreId }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.DatetimeValues)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany(a => a.DatetimeValues)
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== CategoryEntity Configuration ==========
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("CategoryEntity");
            entity.HasKey(e => e.EntityId);
            entity.Property(e => e.EntityId).HasColumnName("entity_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Path).HasColumnName("path").HasMaxLength(255);
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.UrlKey).HasColumnName("url_key").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MetaTitle).HasColumnName("meta_title").HasMaxLength(255);
            entity.Property(e => e.MetaDescription).HasColumnName("meta_description").HasMaxLength(1000);
            entity.Property(e => e.MetaKeywords).HasColumnName("meta_keywords").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== ProductCategoryRelation Configuration ==========
        modelBuilder.Entity<ProductCategoryRelation>(entity =>
        {
            entity.ToTable("ProductCategory");
            entity.HasKey(e => new { e.ProductId, e.CategoryId });
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Position).HasColumnName("position");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CategoryRelations)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.ProductRelations)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductInventory Configuration ==========
        modelBuilder.Entity<ProductInventory>(entity =>
        {
            entity.ToTable("ProductInventory");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.MinQty).HasColumnName("min_qty");
            entity.Property(e => e.MinSaleQty).HasColumnName("min_sale_qty").HasDefaultValue(1);
            entity.Property(e => e.MaxSaleQty).HasColumnName("max_sale_qty").HasDefaultValue(10000);
            entity.Property(e => e.IsInStock).HasColumnName("is_in_stock");
            entity.Property(e => e.ManageStock).HasColumnName("manage_stock").HasDefaultValue(true);
            entity.Property(e => e.BackOrders).HasColumnName("backorders");
            entity.Property(e => e.NotifyStockQty).HasColumnName("notify_stock_qty");
            entity.Property(e => e.QtyIncrement).HasColumnName("qty_increments").HasPrecision(12, 4);

            entity.HasOne(e => e.Product)
                .WithOne(p => p.Inventory)
                .HasForeignKey<ProductInventory>(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductPrice Configuration ==========
        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.ToTable("ProductPrice");
            entity.HasKey(e => e.PriceId);
            entity.Property(e => e.PriceId).HasColumnName("price_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CustomerGroupId).HasColumnName("customer_group_id").HasDefaultValue(0);
            entity.Property(e => e.WebsiteId).HasColumnName("website_id").HasDefaultValue(0);
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(20, 6);
            entity.Property(e => e.SpecialPrice).HasColumnName("special_price").HasPrecision(20, 6);
            entity.Property(e => e.SpecialFromDate).HasColumnName("special_from_date");
            entity.Property(e => e.SpecialToDate).HasColumnName("special_to_date");
            entity.Property(e => e.Qty).HasColumnName("qty").HasDefaultValue(1);
            entity.HasIndex(e => new { e.ProductId, e.CustomerGroupId, e.WebsiteId, e.Qty }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Prices)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== ProductMediaGallery Configuration ==========
        modelBuilder.Entity<ProductMediaGallery>(entity =>
        {
            entity.ToTable("ProductMediaGallery");
            entity.HasKey(e => e.ValueId);
            entity.Property(e => e.ValueId).HasColumnName("value_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AttributeId).HasColumnName("attribute_id");
            entity.Property(e => e.Value).HasColumnName("value").HasMaxLength(255);
            entity.Property(e => e.MediaType).HasColumnName("media_type").HasMaxLength(32).HasDefaultValue("image");
            entity.Property(e => e.StoreId).HasColumnName("store_id").HasDefaultValue(0);
            entity.Property(e => e.Label).HasColumnName("label").HasMaxLength(255);
            entity.Property(e => e.AltText).HasColumnName("alt_text").HasMaxLength(255);
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.Disabled).HasColumnName("disabled");
            entity.Property(e => e.IsBaseImage).HasColumnName("is_base_image");
            entity.Property(e => e.IsSmallImage).HasColumnName("is_small_image");
            entity.Property(e => e.IsThumbnail).HasColumnName("is_thumbnail");
            entity.Property(e => e.IsSwatchImage).HasColumnName("is_swatch_image");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.MediaGallery)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Attribute)
                .WithMany()
                .HasForeignKey(e => e.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== ProductRelation Configuration ==========
        modelBuilder.Entity<ProductRelation>(entity =>
        {
            entity.ToTable("ProductRelation");
            entity.HasKey(e => e.RelationId);
            entity.Property(e => e.RelationId).HasColumnName("relation_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ChildId).HasColumnName("child_id");
            entity.Property(e => e.RelationType).HasColumnName("relation_type").HasMaxLength(32).HasDefaultValue("super");
            entity.HasIndex(e => new { e.ParentId, e.ChildId, e.RelationType }).IsUnique();

            entity.HasOne(e => e.Parent)
                .WithMany(p => p.ChildRelations)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Child)
                .WithMany(p => p.ParentRelations)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ========== ProductWebsite Configuration ==========
        modelBuilder.Entity<ProductWebsite>(entity =>
        {
            entity.ToTable("ProductWebsite");
            entity.HasKey(e => new { e.ProductId, e.WebsiteId });
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.WebsiteId).HasColumnName("website_id");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Websites)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed basic attribute set
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Fixed Guid values for seed data to ensure consistency
        var defaultAttributeSetId = new Guid("11111111-1111-1111-1111-111111111111");
        var generalGroupId = new Guid("22222222-2222-2222-2222-222222222222");
        var pricesGroupId = new Guid("33333333-3333-3333-3333-333333333333");
        var imagesGroupId = new Guid("44444444-4444-4444-4444-444444444444");
        var metaGroupId = new Guid("55555555-5555-5555-5555-555555555555");
        var contentGroupId = new Guid("66666666-6666-6666-6666-666666666666");

        // Seed default attribute set
        modelBuilder.Entity<AttributeSet>().HasData(
            new AttributeSet { AttributeSetId = defaultAttributeSetId, EntityTypeId = 4, AttributeSetName = "Default", SortOrder = 1 }
        );

        // Seed default attribute groups
        modelBuilder.Entity<AttributeGroup>().HasData(
            new AttributeGroup { AttributeGroupId = generalGroupId, AttributeSetId = defaultAttributeSetId, AttributeGroupName = "General", SortOrder = 1 },
            new AttributeGroup { AttributeGroupId = pricesGroupId, AttributeSetId = defaultAttributeSetId, AttributeGroupName = "Prices", SortOrder = 2 },
            new AttributeGroup { AttributeGroupId = imagesGroupId, AttributeSetId = defaultAttributeSetId, AttributeGroupName = "Images", SortOrder = 3 },
            new AttributeGroup { AttributeGroupId = metaGroupId, AttributeSetId = defaultAttributeSetId, AttributeGroupName = "Meta Information", SortOrder = 4 },
            new AttributeGroup { AttributeGroupId = contentGroupId, AttributeSetId = defaultAttributeSetId, AttributeGroupName = "Content", SortOrder = 5 }
        );

        // Seed core attributes
        modelBuilder.Entity<EavAttribute>().HasData(
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000001"), EntityTypeId = 4, AttributeCode = "name", BackendType = "varchar", FrontendInput = "text", FrontendLabel = "Product Name", IsRequired = true, IsSearchable = true, IsVisibleOnFront = true, AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000002"), EntityTypeId = 4, AttributeCode = "description", BackendType = "text", FrontendInput = "textarea", FrontendLabel = "Description", IsSearchable = true, IsHtmlAllowedOnFront = true, AttributeGroupId = contentGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000003"), EntityTypeId = 4, AttributeCode = "short_description", BackendType = "text", FrontendInput = "textarea", FrontendLabel = "Short Description", IsSearchable = true, IsHtmlAllowedOnFront = true, AttributeGroupId = contentGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000004"), EntityTypeId = 4, AttributeCode = "price", BackendType = "decimal", FrontendInput = "price", FrontendLabel = "Price", IsRequired = true, IsSearchable = true, IsFilterable = true, IsComparable = true, AttributeGroupId = pricesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000005"), EntityTypeId = 4, AttributeCode = "special_price", BackendType = "decimal", FrontendInput = "price", FrontendLabel = "Special Price", IsFilterable = true, AttributeGroupId = pricesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000006"), EntityTypeId = 4, AttributeCode = "status", BackendType = "int", FrontendInput = "select", FrontendLabel = "Status", IsRequired = true, DefaultValue = "1", AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000007"), EntityTypeId = 4, AttributeCode = "visibility", BackendType = "int", FrontendInput = "select", FrontendLabel = "Visibility", IsRequired = true, DefaultValue = "4", AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000008"), EntityTypeId = 4, AttributeCode = "weight", BackendType = "decimal", FrontendInput = "text", FrontendLabel = "Weight", AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000009"), EntityTypeId = 4, AttributeCode = "url_key", BackendType = "varchar", FrontendInput = "text", FrontendLabel = "URL Key", IsUnique = true, AttributeGroupId = metaGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000a"), EntityTypeId = 4, AttributeCode = "meta_title", BackendType = "varchar", FrontendInput = "text", FrontendLabel = "Meta Title", AttributeGroupId = metaGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000b"), EntityTypeId = 4, AttributeCode = "meta_description", BackendType = "text", FrontendInput = "textarea", FrontendLabel = "Meta Description", AttributeGroupId = metaGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000c"), EntityTypeId = 4, AttributeCode = "meta_keywords", BackendType = "text", FrontendInput = "textarea", FrontendLabel = "Meta Keywords", AttributeGroupId = metaGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000d"), EntityTypeId = 4, AttributeCode = "image", BackendType = "varchar", FrontendInput = "media_image", FrontendLabel = "Base Image", AttributeGroupId = imagesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000e"), EntityTypeId = 4, AttributeCode = "small_image", BackendType = "varchar", FrontendInput = "media_image", FrontendLabel = "Small Image", AttributeGroupId = imagesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-00000000000f"), EntityTypeId = 4, AttributeCode = "thumbnail", BackendType = "varchar", FrontendInput = "media_image", FrontendLabel = "Thumbnail", AttributeGroupId = imagesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000010"), EntityTypeId = 4, AttributeCode = "color", BackendType = "int", FrontendInput = "select", FrontendLabel = "Color", IsFilterable = true, IsComparable = true, AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000011"), EntityTypeId = 4, AttributeCode = "size", BackendType = "int", FrontendInput = "select", FrontendLabel = "Size", IsFilterable = true, IsComparable = true, AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000012"), EntityTypeId = 4, AttributeCode = "manufacturer", BackendType = "varchar", FrontendInput = "select", FrontendLabel = "Manufacturer", IsFilterable = true, AttributeGroupId = generalGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000013"), EntityTypeId = 4, AttributeCode = "special_from_date", BackendType = "datetime", FrontendInput = "date", FrontendLabel = "Special Price From Date", AttributeGroupId = pricesGroupId },
            new EavAttribute { AttributeId = new Guid("10000000-0000-0000-0000-000000000014"), EntityTypeId = 4, AttributeCode = "special_to_date", BackendType = "datetime", FrontendInput = "date", FrontendLabel = "Special Price To Date", AttributeGroupId = pricesGroupId }
        );
    }
}
