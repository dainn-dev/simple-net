namespace DainnUserManagement.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceType, object key) : base($"{resourceType} with key '{key}' was not found.")
    {
    }
}

