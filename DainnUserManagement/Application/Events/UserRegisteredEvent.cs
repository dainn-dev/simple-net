namespace DainnUserManagement.Application.Events;

/// <summary>
/// Event raised when a user successfully registers.
/// </summary>
public class UserRegisteredEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the registered user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the registered user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the registered user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}

