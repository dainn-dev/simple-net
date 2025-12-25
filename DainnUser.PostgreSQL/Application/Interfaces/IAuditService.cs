namespace DainnUser.PostgreSQL.Application.Interfaces;

/// <summary>
/// Interface for audit logging operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event asynchronously.
    /// </summary>
    /// <param name="action">The action that was performed.</param>
    /// <param name="userId">The unique identifier of the user who performed the action.</param>
    /// <param name="details">Optional additional details about the action.</param>
    /// <param name="ipAddress">The IP address from which the action was performed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(string action, Guid? userId, string? details, string ipAddress);

    /// <summary>
    /// Logs an audit event asynchronously using the client IP from HTTP context.
    /// </summary>
    /// <param name="action">The action that was performed.</param>
    /// <param name="userId">The unique identifier of the user who performed the action.</param>
    /// <param name="details">Optional additional details about the action.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogAsync(string action, Guid? userId, string? details = null);
}

