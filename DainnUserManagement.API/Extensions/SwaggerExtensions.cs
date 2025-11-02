using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using DainnUserManagement.API.Options;

namespace DainnUserManagement.API.Extensions;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation services with JWT Bearer authentication, XML comments, and versioning.
    /// Configuration is read from appsettings.json under the "Swagger" section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="assembly">The assembly containing the API controllers and XML documentation. If null, attempts to auto-detect.</param>
    public static void AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        // Read Swagger options from configuration
        var swaggerOptions = new SwaggerOptions();
        configuration.GetSection("Swagger").Bind(swaggerOptions);

        // Try to auto-detect the entry assembly (usually the consumer application)
        assembly ??= Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();

        services.AddSwaggerGen(c =>
        {
            // Add JWT Bearer authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments
            var xmlFile = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Group actions by controller name - this creates tags for Swagger UI grouping
            c.TagActionsBy(api =>
            {
                string? controllerName = null;
                
                // First try to get controller name from route values
                if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out controllerName) && !string.IsNullOrEmpty(controllerName))
                {
                    return new[] { controllerName };
                }
                
                // Fallback: extract from controller type name
                if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerDescriptor)
                {
                    controllerName = controllerDescriptor.ControllerName;
                    if (!string.IsNullOrEmpty(controllerName))
                    {
                        return new[] { controllerName };
                    }
                }
                
                return new[] { "Default" };
            });
            
            // Add operation filter to ensure tags are properly set on operations
            c.OperationFilter<SwaggerOperationFilter>();
            
            // Sort endpoints by tags (controllers) for better organization
            c.OrderActionsBy(apiDesc =>
            {
                var controller = apiDesc.ActionDescriptor.RouteValues.TryGetValue("controller", out var ctrl) ? ctrl : "Default";
                return $"{controller}_{apiDesc.HttpMethod}";
            });

            // Use problem details schema for error responses
            c.MapType<Microsoft.AspNetCore.Mvc.ProblemDetails>(() => new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["type"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1") },
                    ["title"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("One or more validation errors occurred.") },
                    ["status"] = new OpenApiSchema { Type = "integer", Format = "int32", Example = new Microsoft.OpenApi.Any.OpenApiInteger(400) },
                    ["traceId"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("00-abc123def456-789") },
                    ["errors"] = new OpenApiSchema
                    {
                        Type = "object",
                        AdditionalProperties = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string" }
                        }
                    }
                }
            });

            // Custom schema filters for better DTO documentation
            c.SchemaFilter<SwaggerSchemaFilter>();
            
            // Custom document filter to ensure tags are properly defined and ordered
            c.DocumentFilter<SwaggerTagDocumentFilter>();
            
            // Ensure tags are used for grouping in Swagger UI
            c.DocInclusionPredicate((version, apiDesc) =>
            {
                // This ensures all endpoints are included and properly tagged
                return true;
            });
        });
        
        // Configure versioned Swagger documents using a post-configuration action
        services.AddOptions<SwaggerGenOptions>()
            .Configure<IApiVersionDescriptionProvider>((options, provider) =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Title = swaggerOptions.Title,
                        Version = description.ApiVersion.ToString(),
                        Description = swaggerOptions.Description,
                        Contact = new OpenApiContact
                        {
                            Name = swaggerOptions.ContactName,
                            Email = swaggerOptions.ContactEmail
                        }
                    });
                }
            });
    }

    /// <summary>
    /// Configures Swagger UI middleware with versioning support.
    /// Configuration is read from appsettings.json under the "Swagger" section.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void UseSwaggerDocumentation(this WebApplication app)
    {
        // CRITICAL: UseSwagger and UseSwaggerUI are endpoint-aware middleware.
        // They must be called AFTER MapControllers() to avoid interfering with routing.
        // However, they should NOT interfere with endpoint routing as they're just static file serving.
        app.UseSwagger();
        
        // Read Swagger options from configuration
        var swaggerOptions = new SwaggerOptions();
        app.Configuration.GetSection("Swagger").Bind(swaggerOptions);
        
        var versionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        app.UseSwaggerUI(c =>
        {
            // Add Swagger endpoints for each API version
            foreach (var description in versionDescriptionProvider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
            {
                c.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"{swaggerOptions.Title} {description.ApiVersion}");
            }
            
            c.RoutePrefix = swaggerOptions.RoutePrefix;
            c.DocumentTitle = swaggerOptions.DocumentTitle;
            c.DefaultModelsExpandDepth(-1); // Hide schema section by default
            c.DisplayRequestDuration();
            
            // Ensure tags (controllers) are displayed as separate groups
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            c.EnableDeepLinking();
            c.EnableFilter();
            c.ShowExtensions();
        });
    }
    
}

/// <summary>
/// Custom schema filter to enhance Swagger documentation.
/// </summary>
internal class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Add example values for common types
        if (context.Type == typeof(DateTime))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("O"));
        }
        else if (context.Type == typeof(Guid))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiString(Guid.NewGuid().ToString());
        }
    }
}

/// <summary>
/// Custom operation filter to ensure tags are properly set on all operations.
/// </summary>
internal class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Ensure the operation has tags (controller name)
        if (operation.Tags == null || !operation.Tags.Any())
        {
            var controllerName = context.ApiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var ctrl) 
                ? ctrl 
                : "Default";
            
            operation.Tags = new List<OpenApiTag>
            {
                new OpenApiTag { Name = controllerName }
            };
        }
        
        // Ensure tags have descriptions from controller XML comments if available
        if (context.ApiDescription.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor)
        {
            var controllerType = descriptor.ControllerTypeInfo;
            // Try to get XML documentation for the controller
            var controllerSummary = controllerType.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description;
            if (string.IsNullOrEmpty(controllerSummary))
            {
                // Try to get from XML comments if available
                var xmlSummary = context.SchemaRepository.Schemas.ContainsKey(controllerType.Name) 
                    ? null 
                    : GetXmlSummaryForType(controllerType);
                controllerSummary = xmlSummary;
            }
            
            if (!string.IsNullOrEmpty(controllerSummary) && operation.Tags.Any())
            {
                operation.Tags.First().Description = controllerSummary;
            }
        }
    }
    
    private string? GetXmlSummaryForType(Type type)
    {
        // This is a placeholder - actual XML comment parsing would require additional libraries
        // For now, we'll rely on the document filter to set descriptions
        return null;
    }
}

/// <summary>
/// Custom document filter to ensure tags are properly defined and grouped by controller.
/// </summary>
internal class SwaggerTagDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Collect all unique controller names from the operations with their descriptions
        var controllerTags = new Dictionary<string, OpenApiTag>();
        
        // First pass: collect all tags from operations
        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                if (operation.Tags != null && operation.Tags.Any())
                {
                    foreach (var tag in operation.Tags)
                    {
                        var tagName = tag.Name ?? "Default";
                        if (!controllerTags.ContainsKey(tagName))
                        {
                            controllerTags[tagName] = new OpenApiTag
                            {
                                Name = tagName,
                                Description = tag.Description ?? $"{tagName} operations"
                            };
                        }
                        // Keep the description if one exists
                        else if (!string.IsNullOrEmpty(tag.Description))
                        {
                            controllerTags[tagName].Description = tag.Description;
                        }
                    }
                }
            }
        }
        
        // Clear existing tags and add them in sorted order
        // This ensures tags appear as separate groups in Swagger UI
        swaggerDoc.Tags.Clear();
        foreach (var tag in controllerTags.OrderBy(t => t.Key))
        {
            swaggerDoc.Tags.Add(tag.Value);
        }
    }
}

