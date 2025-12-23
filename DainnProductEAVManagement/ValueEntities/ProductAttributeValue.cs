using DainnProductEAVManagement.Entities;

namespace DainnProductEAVManagement.ValueEntities;

/// <summary>
/// Base abstract class for product attribute values.
/// Each concrete implementation stores values of a specific data type.
/// This enables efficient storage and querying by data type.
/// </summary>
public abstract class ProductAttributeValue
{
    public Guid ValueId { get; set; } // PK
    public Guid EntityId { get; set; } // FK to ProductEntity
    public Guid AttributeId { get; set; } // FK to EavAttribute
    public int StoreId { get; set; } = 0; // 0 = global, >0 = store-specific (multi-store/language support)

    // Navigation
    public ProductEntity Product { get; set; } = null!;
    public EavAttribute Attribute { get; set; } = null!;
}
