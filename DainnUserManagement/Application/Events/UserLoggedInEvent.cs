namespace DainnUserManagement.Application.Events;

/// <summary>
/// Event raised when a user successfully logs in.
/// </summary>
public class UserLoggedInEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the logged-in user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the login occurred.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

