// This file has been moved to DainnCommon.Exceptions.ValidationException
// Keeping this file for backward compatibility - it now references the shared implementation
namespace DainnUser.PostgreSQL.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
[Obsolete("Use DainnCommon.Exceptions.ValidationException instead")]
public class ValidationException : DainnCommon.Exceptions.ValidationException
{
    public ValidationException(System.Collections.Generic.IDictionary<string, string[]> errors) : base(errors)
    {
    }

    public ValidationException(string message) : base(message)
    {
    }
}

