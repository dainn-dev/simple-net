using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DainnUser.PostgreSQL.Application.Dtos;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Integration tests for error flows.
/// Tests: Invalid DTO → 400, Duplicate → 422, Not found → 404
/// </summary>
public class ErrorFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ErrorFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithInvalidDto_Returns400()
    {
        // Arrange
        var invalidDto = new RegisterDto
        {
            Email = "invalid-email", // Invalid email format
            Password = "123", // Too short
            FullName = "" // Empty
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", invalidDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", content);
        
        // Verify ProblemDetails structure
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("type", out _));
        Assert.True(json.TryGetProperty("status", out _));
        Assert.True(json.TryGetProperty("title", out _));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns422()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@example.com";
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = "Test@123!",
            FullName = "Test User"
        };

        // Register first user
        var firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        firstResponse.EnsureSuccessStatusCode();

        // Act - Try to register again with same email
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, secondResponse.StatusCode);
        
        var content = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("traceId", content);
    }

    [Fact]
    public async Task GetUser_WithNonExistentId_Returns404()
    {
        // Arrange - Login as admin
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<DainnUser.PostgreSQL.Domain.Entities.AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<DainnUser.PostgreSQL.Domain.Entities.AppRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new DainnUser.PostgreSQL.Domain.Entities.AppRole { Name = "Admin", Description = "Admin role" });
        }

        var adminEmail = "admin@test.com";
        var adminPassword = "Admin@123!";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new DainnUser.PostgreSQL.Domain.Entities.AppUser { Email = adminEmail, UserName = adminEmail, FullName = "Admin", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto { Email = adminEmail, Password = adminPassword });
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse!.AccessToken);

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/admin/users/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", content);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/account/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

