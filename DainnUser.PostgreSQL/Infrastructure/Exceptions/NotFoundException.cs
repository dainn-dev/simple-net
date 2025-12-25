// This file has been moved to DainnCommon.Exceptions.NotFoundException
// Keeping this file for backward compatibility - it now references the shared implementation
namespace DainnUser.PostgreSQL.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
[Obsolete("Use DainnCommon.Exceptions.NotFoundException instead")]
public class NotFoundException : DainnCommon.Exceptions.NotFoundException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceType, object key) : base(resourceType, key)
    {
    }
}

