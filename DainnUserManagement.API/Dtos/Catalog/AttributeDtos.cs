namespace DainnUserManagement.API.Dtos.Catalog;

/// <summary>
/// DTO for creating a new EAV attribute.
/// </summary>
public class CreateAttributeDto
{
    /// <summary>
    /// The unique attribute code (e.g., "color", "size", "brand").
    /// </summary>
    public string AttributeCode { get; set; } = string.Empty;

    /// <summary>
    /// The backend storage type: varchar, int, decimal, text, datetime.
    /// </summary>
    public string BackendType { get; set; } = "varchar";

    /// <summary>
    /// The frontend input type: text, textarea, select, multiselect, boolean, date, price, media_image.
    /// </summary>
    public string FrontendInput { get; set; } = "text";

    /// <summary>
    /// The display label for the attribute.
    /// </summary>
    public string? FrontendLabel { get; set; }

    /// <summary>
    /// Whether the attribute is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether the attribute value must be unique.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Whether the attribute is searchable.
    /// </summary>
    public bool IsSearchable { get; set; }

    /// <summary>
    /// Whether the attribute can be used as a filter.
    /// </summary>
    public bool IsFilterable { get; set; }

    /// <summary>
    /// Whether the attribute is comparable.
    /// </summary>
    public bool IsComparable { get; set; }

    /// <summary>
    /// The attribute group ID to assign to.
    /// </summary>
    public Guid? AttributeGroupId { get; set; }

    /// <summary>
    /// Default value for the attribute.
    /// </summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// DTO for updating an attribute.
/// </summary>
public class UpdateAttributeDto
{
    public string? FrontendLabel { get; set; }
    public string? FrontendInput { get; set; }
    public bool? IsRequired { get; set; }
    public bool? IsSearchable { get; set; }
    public bool? IsFilterable { get; set; }
    public bool? IsComparable { get; set; }
    public Guid? AttributeGroupId { get; set; }
    public int? Position { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// DTO for attribute response.
/// </summary>
public class AttributeDto
{
    public Guid AttributeId { get; set; }
    public string AttributeCode { get; set; } = string.Empty;
    public string BackendType { get; set; } = string.Empty;
    public string FrontendInput { get; set; } = string.Empty;
    public string? FrontendLabel { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    public bool IsVisibleOnFront { get; set; }
    public int? Position { get; set; }
    public string? DefaultValue { get; set; }
    public Guid? AttributeGroupId { get; set; }
    public string? AttributeGroupName { get; set; }
}

/// <summary>
/// DTO for creating an attribute set.
/// </summary>
public class CreateAttributeSetDto
{
    /// <summary>
    /// The name of the attribute set.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO for attribute set response.
/// </summary>
public class AttributeSetDto
{
    public Guid AttributeSetId { get; set; }
    public string AttributeSetName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AttributeGroupDto>? Groups { get; set; }
}

/// <summary>
/// DTO for creating an attribute group.
/// </summary>
public class CreateAttributeGroupDto
{
    /// <summary>
    /// The name of the attribute group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The attribute set ID this group belongs to.
    /// </summary>
    public Guid AttributeSetId { get; set; }

    /// <summary>
    /// Sort order within the attribute set.
    /// </summary>
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// DTO for attribute group response.
/// </summary>
public class AttributeGroupDto
{
    public Guid AttributeGroupId { get; set; }
    public Guid AttributeSetId { get; set; }
    public string AttributeGroupName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AttributeDto>? Attributes { get; set; }
}
