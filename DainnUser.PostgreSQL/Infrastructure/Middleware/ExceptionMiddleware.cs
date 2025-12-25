// This file has been moved to DainnCommon.Middleware.ExceptionMiddleware
// Keeping this file for backward compatibility - it now references the shared implementation
namespace DainnUser.PostgreSQL.Infrastructure.Middleware;

/// <summary>
/// Middleware to handle and format exceptions globally.
/// </summary>
[Obsolete("Use DainnCommon.Middleware.ExceptionMiddleware instead")]
public class ExceptionMiddleware : DainnCommon.Middleware.ExceptionMiddleware
{
    public ExceptionMiddleware(Microsoft.AspNetCore.Http.RequestDelegate next, Microsoft.Extensions.Logging.ILogger<ExceptionMiddleware> logger) 
        : base(next, logger)
    {
    }
}

