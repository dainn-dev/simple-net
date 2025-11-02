using Microsoft.AspNetCore.Http;
using DainnUserManagement.Application.Interfaces;
using DainnUserManagement.Domain.Entities;
using DainnUserManagement.Infrastructure.Persistence;

namespace DainnUserManagement.Application.Services;

/// <summary>
/// Service for audit logging operations.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Logs an audit event asynchronously.
    /// </summary>
    /// <param name="action">The action that was performed.</param>
    /// <param name="userId">The unique identifier of the user who performed the action.</param>
    /// <param name="details">Optional additional details about the action.</param>
    /// <param name="ipAddress">The IP address from which the action was performed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task LogAsync(string action, Guid? userId, string? details, string ipAddress)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            UserId = userId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the client IP address from the HTTP context.
    /// </summary>
    /// <returns>The client IP address, or "Unknown" if not available.</returns>
    protected virtual string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return "Unknown";
        }

        // Check for forwarded IP address (from proxies/load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].ToString();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Logs an audit event asynchronously using the client IP from HTTP context.
    /// </summary>
    /// <param name="action">The action that was performed.</param>
    /// <param name="userId">The unique identifier of the user who performed the action.</param>
    /// <param name="details">Optional additional details about the action.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task LogAsync(string action, Guid? userId, string? details = null)
    {
        var ipAddress = GetClientIpAddress();
        await LogAsync(action, userId, details, ipAddress);
    }
}

