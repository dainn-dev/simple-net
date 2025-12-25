using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using DainnUser.PostgreSQL.Domain.Entities;
using DainnUser.PostgreSQL.Infrastructure.Persistence;
using DainnUser.PostgreSQL.Options;
using DainnUser.PostgreSQL.Application.Interfaces;
using DainnUser.PostgreSQL.Application.Services;
using DainnUser.PostgreSQL.Infrastructure.Auth;
using DainnUser.PostgreSQL.Application.Events;
using DainnUser.PostgreSQL.Application.Validators;
using DainnUser.PostgreSQL.Infrastructure.Telemetry;
using DainnCommon.Extensions;
using DainnCommon.Middleware;
using FluentValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data.Common;
#if !DISABLE_MYSQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

namespace DainnUser.PostgreSQL.Extensions;

public static class ServiceExtensions
{
    public static void AddDainnUserManagement(this WebApplicationBuilder builder, Action<UserManagementOptions>? configureOptions = null)
    {
        var options = new UserManagementOptions();
        builder.Configuration.GetSection("DainnApplication").Bind(options);
        
        configureOptions?.Invoke(options);

        // Validate required configuration
        ValidateUserManagementConfiguration(options);

        builder.Services.AddDbContext<AppDbContext>(dbOptions =>
        {
            dbOptions.ConfigureDatabaseProvider(options.Provider, options.ConnectionString, options.ConfigureDbContext);
        });

        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // NOTE: Controllers should be registered by the consuming application (e.g., DainnUserManagement.Sample)
        // This library provides the infrastructure and services, but controllers are defined in the sample project
        // We don't call AddControllers() here to avoid conflicts - the consuming app should call it with AddApplicationPart

        // Read API versioning options from configuration
        var apiVersioningConfig = new ApiVersioningConfig();
        builder.Configuration.GetSection("ApiVersioning").Bind(apiVersioningConfig);

        // Add API versioning
        // CRITICAL FIX: For route-based versioning with protected endpoints, we MUST use UrlSegmentApiVersionReader
        // and ensure it's the primary reader. Combining multiple readers can cause routing issues.
        builder.Services.AddApiVersioning(apiVersioningOptions =>
        {
            var defaultVersion = ParseApiVersion(apiVersioningConfig.DefaultVersion);
            apiVersioningOptions.DefaultApiVersion = defaultVersion;
            apiVersioningOptions.AssumeDefaultVersionWhenUnspecified = apiVersioningConfig.AssumeDefaultVersionWhenUnspecified;
            apiVersioningOptions.ReportApiVersions = apiVersioningConfig.ReportApiVersions;
            
            // CRITICAL FIX: Use ONLY UrlSegmentApiVersionReader for route-based versioning.
            // After extensive testing, we discovered that combining multiple API version readers
            // causes routing to fail for protected endpoints with [Authorize].
            //
            // SOLUTION: Use ONLY UrlSegmentApiVersionReader for routes with v{version:apiVersion}.
            // This ensures "api/v1/account/me" correctly matches version "1.0" for protected endpoints.
            apiVersioningOptions.ApiVersionReader = new UrlSegmentApiVersionReader();
            
            // CRITICAL: Configure API versioning to work with endpoint routing
            // The UnsupportedApiVersionStatusCode helps with debugging but doesn't solve the 404 issue
            // The real fix is ensuring proper middleware order and API version reader configuration
        });

        // Add API explorer for Swagger
        builder.Services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = apiVersioningConfig.GroupNameFormat;
            setup.SubstituteApiVersionInUrl = apiVersioningConfig.SubstituteApiVersionInUrl;
        });

        // Add authorization policies
        // CRITICAL FIX: Use AddAuthorizationBuilder() instead of AddAuthorization() for better endpoint routing support
        // This ensures authorization works correctly with endpoint routing and API versioning
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

        // Read JWT validation options from configuration
        var jwtValidation = options.JwtValidation;
        if (jwtValidation == null)
        {
            jwtValidation = new JwtValidationOptions();
            builder.Configuration.GetSection("DainnApplication:JwtValidation").Bind(jwtValidation);
        }

        // CRITICAL: Configure authentication with JWT Bearer
        // The authentication scheme must be properly configured for [Authorize] to work with endpoint routing
        var authBuilder = builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtValidation.ValidateIssuer,
                    ValidateAudience = jwtValidation.ValidateAudience,
                    ValidateLifetime = jwtValidation.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtValidation.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.JwtSecret)),
                    ClockSkew = jwtValidation.ClockSkew,
                    ValidIssuer = !string.IsNullOrEmpty(jwtValidation.ValidIssuer) ? jwtValidation.ValidIssuer : null,
                    ValidAudience = !string.IsNullOrEmpty(jwtValidation.ValidAudience) ? jwtValidation.ValidAudience : null
                };
            });

        // Configure OAuth2 external authentication providers
        var oauth2Settings = options.OAuth2Settings;

        // Google OAuth2
        if (oauth2Settings.Google.Enabled && !string.IsNullOrEmpty(oauth2Settings.Google.ClientId))
        {
            authBuilder.AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = oauth2Settings.Google.ClientId;
                googleOptions.ClientSecret = oauth2Settings.Google.ClientSecret;
                googleOptions.SaveTokens = true;
                
                // Configure scope to get user profile information
                googleOptions.Scope.Add("profile");
                googleOptions.Scope.Add("email");
            });
        }

        // Microsoft OAuth2
        if (oauth2Settings.Microsoft.Enabled && !string.IsNullOrEmpty(oauth2Settings.Microsoft.ClientId))
        {
            authBuilder.AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = oauth2Settings.Microsoft.ClientId;
                microsoftOptions.ClientSecret = oauth2Settings.Microsoft.ClientSecret;
                microsoftOptions.SaveTokens = true;
            });
        }

        // Facebook OAuth2
        if (oauth2Settings.Facebook.Enabled && !string.IsNullOrEmpty(oauth2Settings.Facebook.AppId))
        {
            authBuilder.AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = oauth2Settings.Facebook.AppId;
                facebookOptions.AppSecret = oauth2Settings.Facebook.AppSecret;
                facebookOptions.SaveTokens = true;
            });
        }

        // GitHub OAuth2
        if (oauth2Settings.GitHub.Enabled && !string.IsNullOrEmpty(oauth2Settings.GitHub.ClientId))
        {
            authBuilder.AddGitHub(githubOptions =>
            {
                githubOptions.ClientId = oauth2Settings.GitHub.ClientId;
                githubOptions.ClientSecret = oauth2Settings.GitHub.ClientSecret;
                githubOptions.SaveTokens = true;
            });
        }

        // Apple OAuth2 - NOTE: Apple doesn't have built-in ASP.NET Core support
        // This is a simplified implementation. For production, you may need custom middleware
        // or a third-party library like AspNet.Security.OAuth.Apple
        if (oauth2Settings.Apple.Enabled && !string.IsNullOrEmpty(oauth2Settings.Apple.ClientId))
        {
            // Apple Sign In requires custom implementation
            // You'll need to install a package like AspNet.Security.OAuth.Apple
            // or implement custom OAuth2 flow for Apple
            // For now, we'll log a warning that it requires additional setup
            var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(ServiceExtensions).FullName!);
            logger.LogWarning(
                "Apple OAuth2 is configured but requires additional setup. " +
                "Consider installing AspNet.Security.OAuth.Apple package for production use."
            );
        }

        if (options.CustomUserServiceType != null)
        {
            builder.Services.AddScoped(typeof(IUserService), options.CustomUserServiceType);
        }
        else
        {
            builder.Services.AddScoped<IUserService, UserService>();
        }

        // Register HttpContextAccessor for IP tracking
        builder.Services.AddHttpContextAccessor();

        // Add response caching
        builder.Services.AddResponseCaching();

        // Add CORS if enabled
        if (options.CorsSettings.Enabled && options.CorsSettings.AllowedOrigins.Any())
        {
            builder.Services.AddCors(corsOptions =>
            {
                corsOptions.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(options.CorsSettings.AllowedOrigins.ToArray())
                        .WithMethods(options.CorsSettings.AllowedMethods.ToArray())
                        .WithHeaders(options.CorsSettings.AllowedHeaders.ToArray());
                    
                    if (options.CorsSettings.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                });
            });
        }

        // Add antiforgery tokens
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        // Add rate limiting
        if (options.SecuritySettings.EnableRateLimiting)
        {
            builder.Services.AddRateLimiter(rateLimiterOptions =>
            {
                // Add specific rate limit policies for login and register endpoints
                rateLimiterOptions.AddFixedWindowLimiter("LoginRegisterPolicy", fixedOptions =>
                {
                    fixedOptions.PermitLimit = options.SecuritySettings.RateLimitRequestsPerMinute;
                    fixedOptions.Window = TimeSpan.FromMinutes(1);
                    fixedOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    fixedOptions.QueueLimit = 2;
                });

                // Global rate limiter for other endpoints (optional, can be removed if not needed)
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var path = context.Request.Path.Value ?? string.Empty;
                    // Only apply to login and register endpoints
                    if (path.Contains("/login", StringComparison.OrdinalIgnoreCase) ||
                        path.Contains("/register", StringComparison.OrdinalIgnoreCase))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: path,
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = options.SecuritySettings.RateLimitRequestsPerMinute,
                                Window = TimeSpan.FromMinutes(1),
                                AutoReplenishment = true
                            });
                    }

                    return RateLimitPartition.GetNoLimiter(string.Empty);
                });
            });
        }

        // Register validators from assembly
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

        // Register core services
        builder.Services.AddScoped<IAuthService, JwtService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IPermissionService, PermissionService>();
        builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<IEventPublisher, EventPublisher>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IOAuth2Service, OAuth2Service>();

        // Configure OpenTelemetry from configuration
        var otelOptions = new OpenTelemetryOptions();
        builder.Configuration.GetSection("OpenTelemetry").Bind(otelOptions);
        
        // Only enable OpenTelemetry if explicitly enabled (default is false to prevent silent failures)
        if (otelOptions.Enabled)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    if (otelOptions.EnableAspNetCoreInstrumentation)
                        tracing.AddAspNetCoreInstrumentation();
                    if (otelOptions.EnableHttpClientInstrumentation)
                        tracing.AddHttpClientInstrumentation();
                    if (otelOptions.EnableEntityFrameworkCoreInstrumentation)
                        tracing.AddEntityFrameworkCoreInstrumentation();
                    tracing.AddSource(Telemetry.Source.Name)
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(otelOptions.ServiceName))
                        .AddOtlpExporter(opt => opt.Endpoint = new Uri(otelOptions.OtlpTracesEndpoint));
                })
                .WithMetrics(metrics =>
                {
                    if (otelOptions.EnableAspNetCoreInstrumentation)
                        metrics.AddAspNetCoreInstrumentation();
                    if (otelOptions.EnableHttpClientInstrumentation)
                        metrics.AddHttpClientInstrumentation();
                    if (otelOptions.EnablePrometheusExporter)
                        metrics.AddPrometheusExporter();
                });
        }

        // Register options instance for injection
        builder.Services.AddSingleton(options);

        // Add health checks
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("Database", tags: new[] { "ready" })
            .AddUrlGroup(new Uri("https://api.ipify.org"), "Internet", tags: new[] { "ready" })
            .AddCheck<Infrastructure.HealthChecks.JwtHealthCheck>("JWT", tags: new[] { "ready" });
    }

    /// <summary>
    /// Configures the user management middleware pipeline.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void UseUserManagement(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<UserManagementOptions>();

        // Exception handling middleware (must be early in pipeline)
        app.UseMiddleware<ExceptionMiddleware>();

        // HTTPS redirection
        if (options.EnforceHttps)
        {
            app.UseHttpsRedirection();
        }

        // HSTS (HTTP Strict Transport Security)
        if (options.EnableHsts)
        {
            app.UseHsts();
        }

        // CORS (must be before authentication/authorization)
        if (options.CorsSettings.Enabled)
        {
            app.UseCors();
        }

        // Response caching (must be before UseRouting)
        app.UseResponseCaching();

        // Rate limiting (must be before UseRouting)
        if (options.SecuritySettings.EnableRateLimiting)
        {
            app.UseRateLimiter();
        }

        // CRITICAL: DO NOT call UseAuthentication() or UseAuthorization() here!
        // They MUST be called AFTER UseRouting() in Program.cs because they need
        // access to endpoint metadata which is only available after UseRouting().
        // 
        // The correct order in Program.cs must be:
        // UseUserManagement() -> UseRouting() -> UseAuthentication() -> UseAuthorization() -> MapControllers()
    }

    /// <summary>
    /// Maps additional endpoints like health checks and metrics.
    /// Call this after MapControllers() in your Program.cs
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void MapUserManagementEndpoints(this WebApplication app)
    {
        // Prometheus metrics scraping endpoint
        app.MapPrometheusScrapingEndpoint();

        // Health check endpoints
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("ready")
        });

        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = r => !r.Tags.Contains("ready")
        });
    }

    /// <summary>
    /// Migrates the database automatically if configured.
    /// For PostgreSQL, uses EnsureCreated to respect model configuration (UUID types).
    /// For other providers, uses standard migrations.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var options = services.GetRequiredService<UserManagementOptions>();
            var logger = services.GetRequiredService<ILogger<AppDbContext>>();
            
            // For PostgreSQL, use EnsureCreated to respect OnModelCreating UUID configuration
            // For SQL Server without migrations, also use EnsureCreated
            // For other providers with migrations, use standard migrations
            var providerLower = options.Provider.ToLowerInvariant();
            if (providerLower == "postgresql" || providerLower == "npgsql")
            {
                // Check if tables exist using database metadata (more reliable than querying)
                bool tablesExist = false;
                try
                {
                    if (context.Database.CanConnect())
                    {
                        // Use raw SQL to check if the table exists without throwing exceptions
                        var connection = context.Database.GetDbConnection();
                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection.Open();
                        }
                        try
                        {
                            using var command = connection.CreateCommand();
                            command.CommandText = @"
                                SELECT EXISTS (
                                    SELECT FROM information_schema.tables 
                                    WHERE table_schema = 'public' 
                                    AND table_name = 'AspNetRoles'
                                );";
                            var result = command.ExecuteScalar();
                            tablesExist = result != null && Convert.ToBoolean(result);
                        }
                        finally
                        {
                            if (connection.State == System.Data.ConnectionState.Open)
                            {
                                connection.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If we can't check, assume tables don't exist
                    logger.LogDebug(ex, "Could not check if tables exist, assuming they need to be created.");
                    tablesExist = false;
                }
                
                if (!tablesExist)
                {
                    logger.LogInformation("Creating database schema for PostgreSQL (using UUID types)...");
                    context.Database.EnsureCreated();
                    logger.LogInformation("Database schema created successfully with UUID columns.");
                }
                else
                {
                    logger.LogInformation("Database and tables already exist. Skipping schema creation.");
                }
            }
            else if (providerLower == "sqlserver")
            {
                // For SQL Server, use EnsureCreated by default to avoid provider-specific migration issues
                // Migrations created for SQLite/PostgreSQL won't work with SQL Server
                try
                {
                    // Check if tables already exist using database metadata
                    bool tablesExist = false;
                    if (context.Database.CanConnect())
                    {
                        try
                        {
                            var connection = context.Database.GetDbConnection();
                            if (connection.State != System.Data.ConnectionState.Open)
                            {
                                connection.Open();
                            }
                            try
                            {
                                using var command = connection.CreateCommand();
                                command.CommandText = @"
                                    SELECT CASE 
                                        WHEN EXISTS (
                                            SELECT * FROM INFORMATION_SCHEMA.TABLES 
                                            WHERE TABLE_NAME = 'AspNetRoles'
                                        ) THEN 1 
                                        ELSE 0 
                                    END;";
                                var result = command.ExecuteScalar();
                                tablesExist = result != null && Convert.ToInt32(result) == 1;
                            }
                            finally
                            {
                                if (connection.State == System.Data.ConnectionState.Open)
                                {
                                    connection.Close();
                                }
                            }
                        }
                        catch
                        {
                            tablesExist = false;
                        }
                    }

                    if (!tablesExist)
                    {
                        logger.LogInformation("Creating database schema for SQL Server...");
                        context.Database.EnsureCreated();
                        logger.LogInformation("Database schema created successfully for SQL Server.");
                    }
                    else
                    {
                        logger.LogInformation("Database and tables already exist for SQL Server. Skipping schema creation.");
                    }
                }
                catch (Exception createEx)
                {
                    logger.LogError(createEx, "Failed to create SQL Server database schema. Error: {Error}", createEx.Message);
                    throw;
                }
            }
            else
            {
                // Use standard migrations for SQLite, MySQL, etc.
                logger.LogInformation("Running migrations for {Provider}...", providerLower);
                context.Database.Migrate();
                logger.LogInformation("Migrations completed successfully for {Provider}.", providerLower);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<AppDbContext>>();
            logger.LogError(ex, "An error occurred while migrating the database. ExceptionType: {ExceptionType}", ex.GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Seeds the database with default admin role and user if configured.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var options = services.GetRequiredService<UserManagementOptions>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(ServiceExtensions).FullName!);

            if (!options.SeedDefaultAdmin)
            {
                return;
            }

            SeedDatabaseAsync(context, roleManager, userManager, options, logger).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(ServiceExtensions).FullName!);
            logger.LogError(ex, "An error occurred while seeding the database. ExceptionType: {ExceptionType}", ex.GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Validates that required UserManagement configuration is present.
    /// </summary>
    private static void ValidateUserManagementConfiguration(UserManagementOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            errors.Add(
                "The 'DainnApplication:Provider' configuration is required. " +
                "Please set it to one of: sqlite, sqlserver, postgresql, npgsql, mysql, or inmemory. " +
                "Example: \"DainnApplication\": { \"Provider\": \"postgresql\", \"ConnectionString\": \"...\", ... }");
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            errors.Add(
                "The 'DainnApplication:ConnectionString' configuration is required. " +
                "Please provide a valid database connection string. " +
                "Example: \"DainnApplication\": { \"ConnectionString\": \"Data Source=app.db\", ... }");
        }

        if (string.IsNullOrWhiteSpace(options.JwtSecret))
        {
            errors.Add(
                "The 'DainnApplication:JwtSecret' configuration is required. " +
                "Please provide a secret key with at least 32 characters for security. " +
                "Example: \"DainnApplication\": { \"JwtSecret\": \"YourSuperSecretKeyForJWTTokenGeneration12345678901234567890\", ... }");
        }
        else if (options.JwtSecret.Length < 32)
        {
            errors.Add(
                $"The 'DainnApplication:JwtSecret' must be at least 32 characters long for security. " +
                $"Current length: {options.JwtSecret.Length} characters. " +
                "Please provide a longer secret key.");
        }

        if (errors.Any())
        {
            var errorMessage = "UserManagement configuration is invalid or missing required settings:\n\n" +
                             string.Join("\n\n", errors.Select((e, i) => $"{i + 1}. {e}")) +
                             "\n\nPlease add a 'DainnApplication' section to your appsettings.json with the required configuration.";
            
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// Parses an API version string (e.g., "1.0") into an ApiVersion object.
    /// </summary>
    private static ApiVersion ParseApiVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return new ApiVersion(1, 0);
        }

        var parts = versionString.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor))
        {
            return new ApiVersion(major, minor);
        }

        if (int.TryParse(versionString, out var majorOnly))
        {
            return new ApiVersion(majorOnly, 0);
        }

        return new ApiVersion(1, 0);
    }

    private static async Task SeedDatabaseAsync(
        AppDbContext context,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        UserManagementOptions options,
        ILogger logger)
    {
        // First, verify that database connection is available
        try
        {
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("Cannot connect to database. Skipping database seeding.");
                return;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot connect to database. Skipping database seeding. Error: {Error}", ex.Message);
            return;
        }

        // Verify that tables exist by attempting a provider-agnostic query
        // This works for all providers: PostgreSQL, SQLite, SQL Server, MySQL
        try
        {
            // Use EF Core query to verify tables exist - this is provider-agnostic
            _ = await context.Set<AppRole>().CountAsync();
        }
        catch (Exception ex)
        {
            // Catch any table-related errors from any provider
            var errorMessage = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            var isTableError = 
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("table", StringComparison.OrdinalIgnoreCase) && 
                    (errorMessage.Contains("cannot find", StringComparison.OrdinalIgnoreCase) || 
                     errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase)) ||
                errorMessage.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) || // SQL Server
                errorMessage.Contains("no such table", StringComparison.OrdinalIgnoreCase); // SQLite

            if (isTableError)
            {
                logger.LogWarning(
                    "Database tables do not exist yet. Skipping database seeding. " +
                    "Please apply migrations first. Provider: {Provider}. " +
                    "Use: dotnet ef migrations add InitialCreate --context AppDbContext && " +
                    "dotnet ef database update --context AppDbContext. " +
                    "Error: {Error}",
                    options.Provider, ex.Message);
                return;
            }

            // Re-throw if it's not a table existence error
            logger.LogWarning(ex, "Unexpected error while verifying database tables. Skipping seeding. Error: {Error}", ex.Message);
            return;
        }

        // Check if admin role exists - use provider-agnostic EF Core API
        AppRole? adminRole = null;
        try
        {
            adminRole = await roleManager.FindByNameAsync("Admin");
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            var isTableError = 
                errorMessage.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            if (isTableError)
            {
                logger.LogWarning("Could not query for admin role - tables may not exist. Skipping role seeding.");
                return;
            }
            // Re-throw unexpected errors
            throw;
        }
        if (adminRole == null)
        {
            adminRole = new AppRole
            {
                Name = "Admin",
                Description = "Administrator role with full access",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createRoleResult = await roleManager.CreateAsync(adminRole);
            if (createRoleResult.Succeeded)
            {
                logger.LogInformation("Admin role seeded successfully. RoleName: {RoleName}", adminRole.Name);
            }
            else
            {
                logger.LogError("Failed to seed admin role. RoleName: {RoleName}, Errors: {Errors}", 
                    adminRole.Name, string.Join(", ", createRoleResult.Errors.Select(e => e.Description)));
            }
        }

        // Check if default admin user exists - use provider-agnostic EF Core API
        AppUser? adminUser = null;
        try
        {
            adminUser = await userManager.FindByEmailAsync(options.DefaultAdminEmail);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            var isTableError = 
                errorMessage.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            if (isTableError)
            {
                logger.LogWarning("Could not query for admin user - tables may not exist. Skipping user seeding.");
                return;
            }
            // Re-throw unexpected errors
            throw;
        }
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                Email = options.DefaultAdminEmail,
                UserName = options.DefaultAdminEmail,
                FullName = "Administrator",
                IsActive = true,
                EmailConfirmed = true,
                EmailConfirmedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createUserResult = await userManager.CreateAsync(adminUser, options.DefaultAdminPassword);
            if (createUserResult.Succeeded)
            {
                logger.LogInformation("Default admin user created successfully");

                // Assign admin role to the user
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation("Admin role assigned to default admin user");
                }
                else
                {
                    logger.LogWarning("Failed to assign admin role to default user: {Errors}", 
                        string.Join(", ", addToRoleResult.Errors));
                }
            }
            else
            {
                logger.LogError("Failed to create default admin user: {Errors}", 
                    string.Join(", ", createUserResult.Errors));
            }
        }
        else
        {
            // User exists, ensure they have admin role
                var userRoles = await userManager.GetRolesAsync(adminUser);
                if (!userRoles.Contains("Admin"))
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Admin role assigned to existing user. UserId: {UserId}, Email: {Email}", 
                            adminUser.Id, adminUser.Email);
                    }
                }
        }

        // Seed User role
        AppRole? userRole = null;
        try
        {
            userRole = await roleManager.FindByNameAsync("User");
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            var isTableError = 
                errorMessage.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            if (isTableError)
            {
                logger.LogWarning("Could not query for user role - tables may not exist. Skipping role seeding.");
                return;
            }
            throw;
        }
        
        if (userRole == null)
        {
            userRole = new AppRole
            {
                Name = "User",
                Description = "Standard user role with basic access",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createRoleResult = await roleManager.CreateAsync(userRole);
            if (createRoleResult.Succeeded)
            {
                logger.LogInformation("User role seeded successfully. RoleName: {RoleName}", userRole.Name);
            }
            else
            {
                logger.LogError("Failed to seed user role. RoleName: {RoleName}, Errors: {Errors}", 
                    userRole.Name, string.Join(", ", createRoleResult.Errors.Select(e => e.Description)));
            }
        }

        // Seed default user account
        const string defaultUserEmail = "user@example.com";
        const string defaultUserPassword = "User@123!";
        
        AppUser? regularUser = null;
        try
        {
            regularUser = await userManager.FindByEmailAsync(defaultUserEmail);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            var isTableError = 
                errorMessage.Contains("ORA-00942", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            if (isTableError)
            {
                logger.LogWarning("Could not query for user - tables may not exist. Skipping user seeding.");
                return;
            }
            throw;
        }
        
        if (regularUser == null)
        {
            regularUser = new AppUser
            {
                Email = defaultUserEmail,
                UserName = defaultUserEmail,
                FullName = "Regular User",
                IsActive = true,
                EmailConfirmed = true,
                EmailConfirmedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createUserResult = await userManager.CreateAsync(regularUser, defaultUserPassword);
            if (createUserResult.Succeeded)
            {
                logger.LogInformation("Default user account created successfully. Email: {Email}", defaultUserEmail);

                // Assign User role to the regular user
                var addToRoleResult = await userManager.AddToRoleAsync(regularUser, "User");
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation("User role assigned to default user account");
                }
                else
                {
                    logger.LogWarning("Failed to assign user role to default user: {Errors}", 
                        string.Join(", ", addToRoleResult.Errors));
                }
            }
            else
            {
                logger.LogError("Failed to create default user account: {Errors}", 
                    string.Join(", ", createUserResult.Errors));
            }
        }
        else
        {
            // User exists, ensure they have User role
            var userRoles = await userManager.GetRolesAsync(regularUser);
            if (!userRoles.Contains("User"))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(regularUser, "User");
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation("User role assigned to existing user. UserId: {UserId}, Email: {Email}", 
                        regularUser.Id, regularUser.Email);
                }
            }
        }
    }
}

