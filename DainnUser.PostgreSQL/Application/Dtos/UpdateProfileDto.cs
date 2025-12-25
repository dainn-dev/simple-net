namespace DainnUser.PostgreSQL.Application.Dtos;

/// <summary>
/// Data transfer object for updating user profile.
/// </summary>
public class UpdateProfileDto
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; set; }
}

