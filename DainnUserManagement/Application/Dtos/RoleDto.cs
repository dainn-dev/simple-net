namespace DainnUserManagement.Application.Dtos;

/// <summary>
/// Data transfer object for role information.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of permission names assigned to this role.
    /// </summary>
    public List<string> PermissionNames { get; set; } = new();
}

