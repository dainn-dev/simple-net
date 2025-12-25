using DainnProductEAV.PostgreSQL.ValueEntities;

namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Core product entity - represents a product in the catalog.
/// Uses EAV pattern for flexible attribute storage.
/// </summary>
public class ProductEntity
{
    public Guid EntityId { get; set; } // PK
    public string Sku { get; set; } = string.Empty; // UNIQUE
    public string TypeId { get; set; } = "simple"; // simple, configurable, bundle, grouped, virtual, downloadable
    public Guid AttributeSetId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AttributeSet AttributeSet { get; set; } = null!;
    
    // EAV Value relations
    public ICollection<ProductVarcharValue> VarcharValues { get; set; } = new List<ProductVarcharValue>();
    public ICollection<ProductIntValue> IntValues { get; set; } = new List<ProductIntValue>();
    public ICollection<ProductDecimalValue> DecimalValues { get; set; } = new List<ProductDecimalValue>();
    public ICollection<ProductTextValue> TextValues { get; set; } = new List<ProductTextValue>();
    public ICollection<ProductDatetimeValue> DatetimeValues { get; set; } = new List<ProductDatetimeValue>();
    
    // Other relations
    public ICollection<ProductCategoryRelation> CategoryRelations { get; set; } = new List<ProductCategoryRelation>();
    public ProductInventory? Inventory { get; set; }
    public ICollection<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
    public ICollection<ProductMediaGallery> MediaGallery { get; set; } = new List<ProductMediaGallery>();
    public ICollection<ProductWebsite> Websites { get; set; } = new List<ProductWebsite>();
    
    // For configurable products - parent-child relations
    public ICollection<ProductRelation> ChildRelations { get; set; } = new List<ProductRelation>();
    public ICollection<ProductRelation> ParentRelations { get; set; } = new List<ProductRelation>();
}
