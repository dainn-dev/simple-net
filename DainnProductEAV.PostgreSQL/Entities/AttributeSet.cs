namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Attribute set - groups attributes together for product types.
/// Similar to Magento's attribute sets (e.g., Default, Clothing, Electronics).
/// </summary>
public class AttributeSet
{
    public Guid AttributeSetId { get; set; } // PK
    public int EntityTypeId { get; set; } = 4; // Fixed for Product (catalog_product)
    public string AttributeSetName { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Navigation
    public ICollection<AttributeGroup> Groups { get; set; } = new List<AttributeGroup>();
    public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
}
