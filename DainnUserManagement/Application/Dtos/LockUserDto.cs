using System.ComponentModel.DataAnnotations;

namespace DainnUserManagement.Application.Dtos;

/// <summary>
/// Data transfer object for locking a user account.
/// </summary>
public class LockUserDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the lock will expire.
    /// </summary>
    public DateTime? LockUntil { get; set; }
}

