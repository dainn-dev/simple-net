namespace DainnProductEAV.PostgreSQL.ValueEntities;

/// <summary>
/// Stores varchar (short string) attribute values.
/// Examples: name, short_description, meta_title, url_key, color, manufacturer
/// </summary>
public class ProductVarcharValue : ProductAttributeValue
{
    public string? Value { get; set; }
}
