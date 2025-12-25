using System.ComponentModel.DataAnnotations;

namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for changing user password.
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Gets or sets the current password.
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new password.
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;
}

