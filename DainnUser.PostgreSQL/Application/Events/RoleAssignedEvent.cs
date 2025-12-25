namespace DainnUser.PostgreSQL.Application.Events;

/// <summary>
/// Event raised when a role is assigned to a user.
/// </summary>
public class RoleAssignedEvent : IDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who was assigned the role.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the role that was assigned.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
}

