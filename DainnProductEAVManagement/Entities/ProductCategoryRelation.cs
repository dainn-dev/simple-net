namespace DainnProductEAVManagement.Entities;

/// <summary>
/// Many-to-many relation between products and categories.
/// A product can belong to multiple categories.
/// </summary>
public class ProductCategoryRelation
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }
    public int Position { get; set; } // Sort order within the category

    // Navigation
    public ProductEntity Product { get; set; } = null!;
    public CategoryEntity Category { get; set; } = null!;
}
