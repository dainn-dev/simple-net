namespace DainnUserManagement.API.Dtos.Catalog;

/// <summary>
/// DTO for creating a new product.
/// </summary>
public class CreateProductDto
{
    /// <summary>
    /// The unique stock keeping unit (SKU) for the product.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// The product type: simple, configurable, bundle, grouped, virtual, downloadable.
    /// </summary>
    public string TypeId { get; set; } = "simple";

    /// <summary>
    /// The attribute set ID to use for this product.
    /// </summary>
    public Guid AttributeSetId { get; set; }

    /// <summary>
    /// Initial attribute values to set on the product.
    /// </summary>
    public Dictionary<string, object?>? Attributes { get; set; }
}

/// <summary>
/// DTO for updating a product.
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Optional new SKU for the product.
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Optional new product type.
    /// </summary>
    public string? TypeId { get; set; }

    /// <summary>
    /// Attribute values to update.
    /// </summary>
    public Dictionary<string, object?>? Attributes { get; set; }
}

/// <summary>
/// DTO for product response.
/// </summary>
public class ProductDto
{
    public Guid EntityId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string TypeId { get; set; } = "simple";
    public Guid AttributeSetId { get; set; }
    public string? AttributeSetName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object?> Attributes { get; set; } = new();
    public List<CategorySummaryDto>? Categories { get; set; }
    public ProductInventoryDto? Inventory { get; set; }
}

/// <summary>
/// DTO for product list response (simplified).
/// </summary>
public class ProductListDto
{
    public Guid EntityId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string TypeId { get; set; } = "simple";
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for setting/updating attribute values.
/// </summary>
public class SetAttributeDto
{
    /// <summary>
    /// The attribute code to set.
    /// </summary>
    public string AttributeCode { get; set; } = string.Empty;

    /// <summary>
    /// The value to set.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The store ID for store-specific values. 0 = global.
    /// </summary>
    public int StoreId { get; set; } = 0;
}

/// <summary>
/// DTO for bulk setting attributes.
/// </summary>
public class SetAttributesDto
{
    /// <summary>
    /// Dictionary of attribute codes and their values.
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = new();

    /// <summary>
    /// The store ID for store-specific values. 0 = global.
    /// </summary>
    public int StoreId { get; set; } = 0;
}

/// <summary>
/// DTO for product inventory.
/// </summary>
public class ProductInventoryDto
{
    public int Qty { get; set; }
    public bool IsInStock { get; set; }
    public int MinQty { get; set; }
    public int MinSaleQty { get; set; }
    public int MaxSaleQty { get; set; }
    public bool ManageStock { get; set; }
}

/// <summary>
/// DTO for updating product inventory.
/// </summary>
public class UpdateInventoryDto
{
    public int Qty { get; set; }
    public bool IsInStock { get; set; }
}

/// <summary>
/// DTO for assigning product to category.
/// </summary>
public class AssignCategoryDto
{
    public Guid CategoryId { get; set; }
    public int Position { get; set; } = 0;
}

/// <summary>
/// Category summary for product response.
/// </summary>
public class CategorySummaryDto
{
    public Guid EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UrlKey { get; set; }
}
