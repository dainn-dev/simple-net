namespace DainnUser.PostgreSQL.Application.Events;

/// <summary>
/// Event raised when a user account is locked.
/// </summary>
public class UserLockedEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the locked user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time until which the user is locked.
    /// </summary>
    public DateTime Until { get; set; }
}

