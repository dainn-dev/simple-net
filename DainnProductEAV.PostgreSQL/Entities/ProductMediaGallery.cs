namespace DainnProductEAV.PostgreSQL.Entities;

/// <summary>
/// Product media gallery - stores images and videos for products.
/// </summary>
public class ProductMediaGallery
{
    public Guid ValueId { get; set; } // PK
    public Guid ProductId { get; set; }
    public Guid AttributeId { get; set; } // Usually media_gallery attribute
    public string Value { get; set; } = string.Empty; // Image path or video URL
    public string MediaType { get; set; } = "image"; // image, external-video
    public int StoreId { get; set; } = 0;
    public string? Label { get; set; }
    public string? AltText { get; set; }
    public int Position { get; set; }
    public bool Disabled { get; set; }

    // Image roles
    public bool IsBaseImage { get; set; }
    public bool IsSmallImage { get; set; }
    public bool IsThumbnail { get; set; }
    public bool IsSwatchImage { get; set; }

    // Navigation
    public ProductEntity Product { get; set; } = null!;
    public EavAttribute Attribute { get; set; } = null!;
}
