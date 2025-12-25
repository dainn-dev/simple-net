// This file has been moved to DainnCommon.Exceptions.BusinessRuleException
// Keeping this file for backward compatibility - it now references the shared implementation
namespace DainnUser.PostgreSQL.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
[Obsolete("Use DainnCommon.Exceptions.BusinessRuleException instead")]
public class BusinessRuleException : DainnCommon.Exceptions.BusinessRuleException
{
    public BusinessRuleException(string message) : base(message)
    {
    }

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

