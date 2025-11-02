namespace DainnUserManagement.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between roles and permissions.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// Gets or sets the foreign key to the role.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the permission.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the role.
    /// </summary>
    public AppRole Role { get; set; } = null!;

    /// <summary>
    /// Gets or sets the navigation property to the permission.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}

