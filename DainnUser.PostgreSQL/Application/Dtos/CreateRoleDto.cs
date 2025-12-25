using System.ComponentModel.DataAnnotations;

namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for creating a new role.
/// </summary>
public class CreateRoleDto
{
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional description of the role.
    /// </summary>
    public string? Description { get; set; }
}

