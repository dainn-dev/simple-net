namespace DainnProductEAVManagement.ValueEntities;

/// <summary>
/// Stores decimal attribute values.
/// Examples: price, special_price, weight, cost, msrp
/// </summary>
public class ProductDecimalValue : ProductAttributeValue
{
    public decimal? Value { get; set; }
}
