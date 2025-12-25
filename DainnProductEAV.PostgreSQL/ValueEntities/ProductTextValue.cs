namespace DainnProductEAV.PostgreSQL.ValueEntities;

/// <summary>
/// Stores text (long string) attribute values.
/// Examples: description, meta_description, custom_design_from
/// </summary>
public class ProductTextValue : ProductAttributeValue
{
    public string? Value { get; set; }
}
