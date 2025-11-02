using Microsoft.AspNetCore.Identity;

namespace DainnUserManagement.Domain.Entities;

/// <summary>
/// Represents an application role with extended properties for role-based access control.
/// </summary>
public class AppRole : IdentityRole<Guid>
{
    /// <summary>
    /// Gets or sets an optional description of the role's purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the role was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of role-permission relationships for this role.
    /// </summary>
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}

