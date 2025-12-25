using System.ComponentModel.DataAnnotations;

namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for importing user data.
/// </summary>
public class ImportUserDto
{
    /// <summary>
    /// Gets or sets the email address for the user account.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the user account. If not provided, a random password will be generated.
    /// </summary>
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [Required(ErrorMessage = "Full name is required")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the email is confirmed.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets the collection of role names to assign to the user.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

