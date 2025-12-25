using DainnUser.PostgreSQL.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure User Management for testing
builder.AddDainnUserManagement(options =>
{
    options.Provider = "inmemory";
    options.ConnectionString = $"TestDb_{Guid.NewGuid()}";
    options.JwtSecret = "TestJwtSecretKeyForIntegrationTests12345678901234567890";
    options.AutoMigrateDatabase = false;
    options.SeedDefaultAdmin = true;
    options.DefaultAdminEmail = "admin@test.com";
    options.DefaultAdminPassword = "Admin@123!";
    options.SecuritySettings.EnableRateLimiting = false; // Disable for tests
    options.EnforceHttps = false;
    options.EnableHsts = false;
});

var app = builder.Build();

// Configure middleware
app.UseUserManagement();

// Map controllers
app.MapControllers();

app.Run();

// Make Program accessible to WebApplicationFactory
public partial class Program { }

