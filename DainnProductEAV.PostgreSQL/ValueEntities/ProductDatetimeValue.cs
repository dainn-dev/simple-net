namespace DainnProductEAV.PostgreSQL.ValueEntities;

/// <summary>
/// Stores datetime attribute values.
/// Examples: special_from_date, special_to_date, news_from_date, news_to_date, custom_design_from
/// </summary>
public class ProductDatetimeValue : ProductAttributeValue
{
    public DateTime? Value { get; set; }
}
