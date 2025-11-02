namespace DainnUserManagement.Options;

/// <summary>
/// Configuration settings for API versioning.
/// </summary>
public class ApiVersioningConfig
{
    /// <summary>
    /// Gets or sets the default API version (e.g., "1.0").
    /// </summary>
    public string DefaultVersion { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets whether to assume the default version when not specified.
    /// </summary>
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to report API versions in response headers.
    /// </summary>
    public bool ReportApiVersions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable query string version reader.
    /// </summary>
    public bool EnableQueryStringReader { get; set; } = true;

    /// <summary>
    /// Gets or sets the query string parameter name for version (default: "api-version").
    /// </summary>
    public string QueryStringParameterName { get; set; } = "api-version";

    /// <summary>
    /// Gets or sets whether to enable header version reader.
    /// </summary>
    public bool EnableHeaderReader { get; set; } = true;

    /// <summary>
    /// Gets or sets the header name for version (default: "api-version").
    /// </summary>
    public string HeaderName { get; set; } = "api-version";

    /// <summary>
    /// Gets or sets whether to enable URL segment version reader.
    /// </summary>
    public bool EnableUrlSegmentReader { get; set; } = true;

    /// <summary>
    /// Gets or sets the API explorer group name format (default: "'v'VVV").
    /// </summary>
    public string GroupNameFormat { get; set; } = "'v'VVV";

    /// <summary>
    /// Gets or sets whether to substitute API version in URL.
    /// </summary>
    public bool SubstituteApiVersionInUrl { get; set; } = true;
}

