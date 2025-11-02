namespace DainnUserManagement.API.Options;

/// <summary>
/// Configuration options for Swagger/OpenAPI documentation.
/// </summary>
public class SwaggerOptions
{
    /// <summary>
    /// Gets or sets the API title.
    /// </summary>
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// Gets or sets the API description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact name.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact email.
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Swagger UI route prefix.
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// Gets or sets the Swagger UI document title.
    /// </summary>
    public string DocumentTitle { get; set; } = "API Documentation";
}

