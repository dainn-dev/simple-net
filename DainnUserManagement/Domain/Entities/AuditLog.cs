namespace DainnUserManagement.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking user actions and changes.
/// </summary>
public class AuditLog
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit log.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the type of action that was performed (e.g., "Login", "Update", "Delete").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional details about the action performed.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the action was performed.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the audit log entry was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the foreign key to the user associated with this audit log.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property to the user associated with this audit log.
    /// </summary>
    public AppUser? User { get; set; }
}

