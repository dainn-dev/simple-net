namespace DainnUser.PostgreSQL.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when input validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationException(System.Collections.Generic.IDictionary<string, string[]> errors) : base("One or more validation errors occurred")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }

    public ValidationException(string message) : base(message)
    {
    }
}

