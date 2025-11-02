using Microsoft.AspNetCore.Identity;

namespace DainnUserManagement.Domain.Entities;

/// <summary>
/// Represents a permission that can be assigned to roles.
/// </summary>
public class Permission
{
    /// <summary>
    /// Gets or sets the unique identifier for the permission.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the permission.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description of what the permission allows.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets when the permission was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the permission was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of role-permission relationships for this permission.
    /// </summary>
    public ICollection<RolePermission> Roles { get; set; } = new List<RolePermission>();
}

