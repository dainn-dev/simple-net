namespace DainnUserManagement.Application.Events;

/// <summary>
/// Event raised when a user changes their password.
/// </summary>
public class PasswordChangedEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user whose password was changed.
    /// </summary>
    public Guid UserId { get; set; }
}

