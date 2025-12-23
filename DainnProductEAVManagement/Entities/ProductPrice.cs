namespace DainnProductEAVManagement.Entities;

/// <summary>
/// Product price - supports tier pricing, customer group pricing, and multi-website pricing.
/// </summary>
public class ProductPrice
{
    public Guid PriceId { get; set; } // PK
    public Guid ProductId { get; set; }
    public int CustomerGroupId { get; set; } = 0; // 0 = all groups
    public int WebsiteId { get; set; } = 0; // 0 = all websites
    public decimal Price { get; set; }
    public decimal? SpecialPrice { get; set; }
    public DateTime? SpecialFromDate { get; set; }
    public DateTime? SpecialToDate { get; set; }
    public int Qty { get; set; } = 1; // For tier pricing - quantity threshold

    // Navigation
    public ProductEntity Product { get; set; } = null!;
}
