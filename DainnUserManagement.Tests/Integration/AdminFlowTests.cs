using System.Net;
using System.Net.Http.Json;
using DainnUserManagement.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DainnUserManagement.Domain.Entities;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Integration tests for Admin flow.
/// Tests: Login as admin → List users → Lock user → Assign role
/// </summary>
public class AdminFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminFlow_LoginAsAdmin_ListUsers_LockUser_AssignRole()
    {
        // Arrange - Create admin user
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Admin", Description = "Admin role" });
        }

        // Create admin user
        var adminEmail = "admin@test.com";
        var adminPassword = "Admin@123!";
        var adminUser = new AppUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            FullName = "Admin User",
            EmailConfirmed = true,
            IsActive = true
        };

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Act & Assert: Login as Admin
        var loginDto = new LoginDto
        {
            Email = adminEmail,
            Password = adminPassword
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        // Act & Assert: List Users
        var usersResponse = await _client.GetAsync("/api/v1/admin/users");
        usersResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        var usersJson = await usersResponse.Content.ReadAsStringAsync();
        Assert.Contains("items", usersJson);
        Assert.Contains("totalCount", usersJson);

        // Create a regular user to test with
        var testUser = new AppUser
        {
            Email = $"testuser_{Guid.NewGuid()}@example.com",
            UserName = $"testuser_{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            EmailConfirmed = true,
            IsActive = true
        };
        await userManager.CreateAsync(testUser, "Test@123!");
        var testUserId = testUser.Id;

        // Act & Assert: Lock User
        var lockDto = new LockUserDto
        {
            UserId = testUserId,
            LockUntil = DateTime.UtcNow.AddDays(1)
        };

        var lockResponse = await _client.PostAsJsonAsync($"/api/v1/admin/users/{testUserId}/lock", lockDto);
        lockResponse.EnsureSuccessStatusCode();

        // Verify user is locked
        var lockedUser = await userManager.FindByIdAsync(testUserId.ToString());
        Assert.NotNull(lockedUser);
        Assert.False(lockedUser.IsActive);

        // Act & Assert: Assign Role
        var assignRoleDto = new AssignRoleDto
        {
            UserId = testUserId,
            RoleName = "Admin"
        };

        var assignRoleResponse = await _client.PostAsJsonAsync("/api/v1/admin/roles/assign", assignRoleDto);
        assignRoleResponse.EnsureSuccessStatusCode();

        // Verify role was assigned
        var userRoles = await userManager.GetRolesAsync(testUser);
        Assert.Contains("Admin", userRoles);
    }

    [Fact]
    public async Task AdminFlow_GetUserById_ReturnsCorrectUser()
    {
        // Arrange - Login as admin
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Admin", Description = "Admin role" });
        }

        var adminEmail = "admin@test.com";
        var adminPassword = "Admin@123!";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new AppUser { Email = adminEmail, UserName = adminEmail, FullName = "Admin", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto { Email = adminEmail, Password = adminPassword });
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse!.AccessToken);

        // Create test user
        var testUser = new AppUser
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            UserName = $"test_{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(testUser, "Test@123!");

        // Act
        var response = await _client.GetAsync($"/api/v1/admin/users/{testUser.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(user);
        Assert.Equal(testUser.Id, user.Id);
        Assert.Equal(testUser.Email, user.Email);
    }
}

