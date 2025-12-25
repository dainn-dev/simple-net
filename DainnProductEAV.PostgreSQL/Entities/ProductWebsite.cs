namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Product website relation - associates products with websites for multi-website support.
/// </summary>
public class ProductWebsite
{
    public Guid ProductId { get; set; }
    public int WebsiteId { get; set; }

    // Navigation
    public ProductEntity Product { get; set; } = null!;
}
