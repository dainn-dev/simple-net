namespace DainnUserManagement.Application.Events;

/// <summary>
/// Event raised when a user updates their profile.
/// </summary>
public class ProfileUpdatedEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user whose profile was updated.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the previous email address, if it was changed.
    /// </summary>
    public string? OldEmail { get; set; }
}

