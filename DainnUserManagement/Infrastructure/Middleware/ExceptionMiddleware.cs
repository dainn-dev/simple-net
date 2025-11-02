using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DainnUserManagement.Infrastructure.Exceptions;

namespace DainnUserManagement.Infrastructure.Middleware;

/// <summary>
/// Middleware to handle and format exceptions globally.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var response = context.Response;
        response.ContentType = "application/json";

        ErrorResponse errorResponse;

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse
                {
                    Title = "Validation Error",
                    Status = response.StatusCode,
                    TraceId = traceId,
                    Errors = validationEx.Errors
                };
                break;

            case BusinessRuleException businessEx:
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = new ErrorResponse
                {
                    Title = "Business Rule Violation",
                    Status = response.StatusCode,
                    TraceId = traceId,
                    Detail = businessEx.Message
                };
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new ErrorResponse
                {
                    Title = "Resource Not Found",
                    Status = response.StatusCode,
                    TraceId = traceId,
                    Detail = notFoundEx.Message
                };
                break;

            default:
                _logger.LogError(exception, 
                    "An unexpected error occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}", 
                    traceId, context.Request.Path, context.Request.Method);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new ErrorResponse
                {
                    Title = "Internal Server Error",
                    Status = response.StatusCode,
                    TraceId = traceId,
                    Detail = "An unexpected error occurred"
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }

    private class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

