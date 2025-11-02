using System.ComponentModel.DataAnnotations;

namespace DainnUserManagement.Application.Dtos;

/// <summary>
/// Data transfer object for assigning a role to a user.
/// </summary>
public class AssignRoleDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role to assign.
    /// </summary>
    [Required(ErrorMessage = "Role name is required")]
    public string RoleName { get; set; } = string.Empty;
}

