namespace DainnProductEAVManagement.Entities;

/// <summary>
/// Product inventory - stock management for products.
/// Simplified inventory model; can be extended for multi-source inventory.
/// </summary>
public class ProductInventory
{
    public Guid ProductId { get; set; } // PK, FK
    public int Qty { get; set; }
    public int MinQty { get; set; } // Minimum quantity for out-of-stock
    public int MinSaleQty { get; set; } = 1; // Minimum quantity for sale
    public int MaxSaleQty { get; set; } = 10000; // Maximum quantity for sale
    public bool IsInStock { get; set; } = true;
    public bool ManageStock { get; set; } = true;
    public bool BackOrders { get; set; } // Allow backorders
    public int? NotifyStockQty { get; set; } // Low stock notification threshold
    public decimal? QtyIncrement { get; set; } = 1; // Quantity increment step

    // Navigation
    public ProductEntity Product { get; set; } = null!;
}
