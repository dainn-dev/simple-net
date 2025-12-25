using System.ComponentModel.DataAnnotations;

namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for forgot password request.
/// </summary>
public class ForgotPasswordDto
{
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
}

