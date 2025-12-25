namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Attribute group - organizes attributes within an attribute set.
/// Examples: General, Prices, Images, Meta Information, etc.
/// </summary>
public class AttributeGroup
{
    public Guid AttributeGroupId { get; set; } // PK
    public Guid AttributeSetId { get; set; }
    public string AttributeGroupName { get; set; } = string.Empty; // General, Prices, Images...
    public int SortOrder { get; set; }

    // Navigation
    public AttributeSet AttributeSet { get; set; } = null!;
    public ICollection<EavAttribute> Attributes { get; set; } = new List<EavAttribute>();
}
