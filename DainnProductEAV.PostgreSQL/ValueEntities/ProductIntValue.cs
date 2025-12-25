namespace DainnProductEAV.PostgreSQL.ValueEntities;

/// <summary>
/// Stores integer attribute values.
/// Examples: status, visibility, tax_class_id, is_featured, position
/// </summary>
public class ProductIntValue : ProductAttributeValue
{
    public int? Value { get; set; }
}
