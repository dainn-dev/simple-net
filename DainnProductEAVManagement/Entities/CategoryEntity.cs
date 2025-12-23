namespace DainnProductEAVManagement.Entities;

/// <summary>
/// Category entity - hierarchical category structure for organizing products.
/// Uses nested set model with path and level for efficient tree queries.
/// </summary>
public class CategoryEntity
{
    public Guid EntityId { get; set; } // PK
    public Guid? ParentId { get; set; }
    public string Path { get; set; } = string.Empty; // e.g., "1/2/5" for hierarchical path
    public int Level { get; set; } // Depth in the tree (root = 0)
    public int Position { get; set; } // Sort order among siblings
    public bool IsActive { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public string? UrlKey { get; set; }
    public string? Description { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Self-referencing navigation for parent-child
    public CategoryEntity? Parent { get; set; }
    public ICollection<CategoryEntity> Children { get; set; } = new List<CategoryEntity>();
    
    // Product relations
    public ICollection<ProductCategoryRelation> ProductRelations { get; set; } = new List<ProductCategoryRelation>();
}
