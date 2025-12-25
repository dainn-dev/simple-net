using System.ComponentModel.DataAnnotations;

namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for verifying two-factor authentication code.
/// </summary>
public class Verify2FADto
{
    /// <summary>
    /// Gets or sets the two-factor authentication code.
    /// </summary>
    [Required(ErrorMessage = "2FA code is required")]
    public string Code { get; set; } = string.Empty;
}

