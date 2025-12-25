using DainnProductEAV.PostgreSQL.ValueEntities;

namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// EAV Attribute definition - defines the metadata for an attribute.
/// This is the core of the EAV pattern - attributes are defined here and values are stored in type-specific tables.
/// </summary>
public class EavAttribute
{
    public Guid AttributeId { get; set; } // PK
    public int EntityTypeId { get; set; } = 4; // Fixed for Product (catalog_product)
    public string AttributeCode { get; set; } = string.Empty; // name, price, color, description...
    public string BackendType { get; set; } = string.Empty; // varchar, int, decimal, text, datetime
    public string FrontendInput { get; set; } = string.Empty; // text, select, multiselect, textarea, boolean, date, price, media_image
    public string? FrontendLabel { get; set; } // Display label
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    public bool IsVisibleOnFront { get; set; } = true;
    public bool IsHtmlAllowedOnFront { get; set; }
    public bool IsUsedForPromoRules { get; set; }
    public int? Position { get; set; }
    public string? DefaultValue { get; set; }
    public string? Note { get; set; }

    // Optional: Group assignment
    public Guid? AttributeGroupId { get; set; }
    public AttributeGroup? Group { get; set; }

    // Navigation to values (for querying)
    public ICollection<ProductVarcharValue> VarcharValues { get; set; } = new List<ProductVarcharValue>();
    public ICollection<ProductIntValue> IntValues { get; set; } = new List<ProductIntValue>();
    public ICollection<ProductDecimalValue> DecimalValues { get; set; } = new List<ProductDecimalValue>();
    public ICollection<ProductTextValue> TextValues { get; set; } = new List<ProductTextValue>();
    public ICollection<ProductDatetimeValue> DatetimeValues { get; set; } = new List<ProductDatetimeValue>();
}
