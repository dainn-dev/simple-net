namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Product relation - for configurable/grouped products (parent-child relationships)
/// and related/upsell/crosssell product links.
/// </summary>
public class ProductRelation
{
    public Guid RelationId { get; set; } // PK
    public Guid ParentId { get; set; }
    public Guid ChildId { get; set; }
    public string RelationType { get; set; } = "super"; // super (configurable), grouped, related, upsell, crosssell

    // Navigation
    public ProductEntity Parent { get; set; } = null!;
    public ProductEntity Child { get; set; } = null!;
}
