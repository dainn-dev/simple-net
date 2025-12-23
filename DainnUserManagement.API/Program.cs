using DainnUserManagement.Extensions;
using DainnUserManagement.API.Controllers;
using DainnUserManagement.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CRITICAL: Add controllers FIRST, then add User Management library
// This ensures controllers are registered before any library configuration that might affect routing
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AccountController).Assembly); // Reference controller assembly

// Add User Management (library) AFTER controllers are registered
// NOTE: AddDainnUserManagement() doesn't call AddControllers() anymore to avoid conflicts
builder.AddDainnUserManagement();

// Configure Serilog logging
builder.Host.UseSerilogLogging();

// Add Swagger documentation
builder.Services.AddSwaggerDocumentation(builder.Configuration);

var app = builder.Build();

// Database migration (if enabled) - MUST happen before any middleware that accesses the database
if (app.Configuration.GetValue<bool>("UserManagement:AutoMigrateDatabase"))
{
    app.MigrateDatabase();
}

// Database seeding (if enabled) - MUST happen after migration
if (app.Configuration.GetValue<bool>("UserManagement:SeedDefaultAdmin"))
{
    app.SeedDatabase();
}

// Configure the HTTP request pipeline
// CRITICAL FIX: In .NET 6+, for endpoint routing with protected endpoints and API versioning:
// The correct order MUST be:
// 1. UseUserManagement() - Sets up infrastructure (but NOT authentication/authorization yet)
// 2. UseRouting() - Creates routing context (MUST be first, before auth middleware)
// 3. UseAuthentication() and UseAuthorization() - Need endpoint metadata from UseRouting()
// 4. MapControllers() - Maps endpoints and completes the routing pipeline
//
// IMPORTANT: UseAuthentication() and UseAuthorization() MUST come AFTER UseRouting()
// because they need access to endpoint metadata to determine authorization requirements.
app.UseUserManagement();

// CRITICAL FIX: Protected endpoints with [Authorize] were returning 404.
// After extensive debugging, the solution is to use UseEndpoints() explicitly
// to ensure the routing branch is properly completed for protected endpoints.
//
// The correct middleware order for .NET 6+ with API versioning and authorization:
// 1. UseRouting() - Establishes routing context
// 2. UseAuthentication() - Authenticates (needs routing context)
// 3. UseAuthorization() - Authorizes (needs routing context and auth)
// 4. UseEndpoints() - Completes routing branch (must be explicit)
//
// NOTE: MapControllers() internally uses UseEndpoints(), but for protected endpoints
// with API versioning, we need to be more explicit about completing the routing branch.
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// CRITICAL FIX: In .NET 6+, MapControllers() handles endpoint routing automatically.
// UseEndpoints() is the old approach - MapControllers() is the new way and should work.
// If we still get 404, the issue is likely with API versioning + authorization interaction,
// not the middleware order itself.
app.MapControllers();

// Configure Swagger UI (must be after MapControllers)
app.UseSwaggerDocumentation();

// Map additional endpoints (health checks, metrics)
app.MapUserManagementEndpoints();



app.Run();
