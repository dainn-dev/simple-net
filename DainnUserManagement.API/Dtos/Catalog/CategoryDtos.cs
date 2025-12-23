namespace DainnUserManagement.API.Dtos.Catalog;

/// <summary>
/// DTO for creating a new category.
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// The category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The parent category ID. Null for root categories.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Optional URL key for SEO-friendly URLs.
    /// </summary>
    public string? UrlKey { get; set; }

    /// <summary>
    /// Optional category description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the category is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sort order position.
    /// </summary>
    public int Position { get; set; } = 0;

    /// <summary>
    /// Meta title for SEO.
    /// </summary>
    public string? MetaTitle { get; set; }

    /// <summary>
    /// Meta description for SEO.
    /// </summary>
    public string? MetaDescription { get; set; }
}

/// <summary>
/// DTO for updating a category.
/// </summary>
public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? UrlKey { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public int? Position { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

/// <summary>
/// DTO for moving a category.
/// </summary>
public class MoveCategoryDto
{
    /// <summary>
    /// The new parent category ID.
    /// </summary>
    public Guid NewParentId { get; set; }

    /// <summary>
    /// The position within the new parent.
    /// </summary>
    public int Position { get; set; } = 0;
}

/// <summary>
/// DTO for category response.
/// </summary>
public class CategoryDto
{
    public Guid EntityId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UrlKey { get; set; }
    public string? Description { get; set; }
    public string Path { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryDto>? Children { get; set; }
}

/// <summary>
/// DTO for category list response (simplified).
/// </summary>
public class CategoryListDto
{
    public Guid EntityId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? UrlKey { get; set; }
    public int Level { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
    public int ChildCount { get; set; }
}
