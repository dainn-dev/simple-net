using System.ComponentModel.DataAnnotations;

namespace DainnUserManagement.Application.Dtos;

/// <summary>
/// Data transfer object for email confirmation request.
/// </summary>
public class ConfirmEmailDto
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the confirmation token.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}

