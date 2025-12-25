namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for admin view of user information.
/// </summary>
public class UserAdminDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the collection of role names assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}

