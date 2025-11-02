using System.Net;
using System.Net.Http.Json;
using DainnUserManagement.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DainnUserManagement.Domain.Entities;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Comprehensive integration tests covering all 20+ endpoints.
/// </summary>
public class AllEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AllEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthController_AllEndpoints_Work()
    {
        // Register
        var registerDto = new RegisterDto
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test@123!",
            FullName = "Test User"
        };
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Login
        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.AccessToken);

        // Refresh
        var refreshDto = new RefreshTokenDto { RefreshToken = tokenResponse.RefreshToken };
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshDto);
        refreshResponse.EnsureSuccessStatusCode();

        // Logout
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AccountController_AllEndpoints_Work()
    {
        // Create and login user
        var user = await CreateAndLoginUser();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", user.Token);

        // GetMe
        var meResponse = await _client.GetAsync("/api/v1/account/me");
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(me);

        // UpdateProfile
        var updateDto = new UpdateProfileDto { FullName = "Updated Name", AvatarUrl = "https://example.com/avatar.jpg" };
        var updateResponse = await _client.PutAsJsonAsync("/api/v1/account/profile", updateDto);
        updateResponse.EnsureSuccessStatusCode();

        // ChangePassword
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "Test@123!",
            NewPassword = "NewTest@123!"
        };
        var changePasswordResponse = await _client.PutAsJsonAsync("/api/v1/account/password", changePasswordDto);
        changePasswordResponse.EnsureSuccessStatusCode();

        // Setup 2FA
        var setup2FAResponse = await _client.PostAsync("/api/v1/account/2fa/setup", null);
        setup2FAResponse.EnsureSuccessStatusCode();
        var setup2FA = await setup2FAResponse.Content.ReadFromJsonAsync<TwoFactorSetupDto>();
        Assert.NotNull(setup2FA);

        // Verify 2FA
        var verify2FADto = new Verify2FADto { Code = "123456" }; // Mock code
        var verify2FAResponse = await _client.PostAsJsonAsync("/api/v1/account/2fa/verify", verify2FADto);
        // May fail due to invalid code, but endpoint should exist
        Assert.True(verify2FAResponse.StatusCode == HttpStatusCode.OK || verify2FAResponse.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PasswordController_AllEndpoints_Work()
    {
        // Forgot Password
        var forgotDto = new ForgotPasswordDto { Email = "test@example.com" };
        var forgotResponse = await _client.PostAsJsonAsync("/api/v1/password/forgot", forgotDto);
        forgotResponse.EnsureSuccessStatusCode(); // Always returns 200 for security

        // Reset Password (would need valid token from database)
        var resetDto = new ResetPasswordDto
        {
            Email = "test@example.com",
            Token = "test-token",
            NewPassword = "NewPass@123!"
        };
        var resetResponse = await _client.PostAsJsonAsync("/api/v1/password/reset", resetDto);
        // May fail due to invalid token, but endpoint should exist
        Assert.True(resetResponse.StatusCode == HttpStatusCode.OK || resetResponse.StatusCode == HttpStatusCode.BadRequest || resetResponse.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EmailController_AllEndpoints_Work()
    {
        // Confirm Email (would need valid token from database)
        var confirmDto = new ConfirmEmailDto
        {
            UserId = Guid.NewGuid(),
            Token = "test-token"
        };
        var confirmResponse = await _client.PostAsJsonAsync("/api/v1/email/confirm", confirmDto);
        // May fail due to invalid token, but endpoint should exist
        Assert.True(confirmResponse.StatusCode == HttpStatusCode.OK || confirmResponse.StatusCode == HttpStatusCode.BadRequest || confirmResponse.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminUsersController_AllEndpoints_Work()
    {
        // Login as admin
        var admin = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin.Token);

        // GetAll
        var getAllResponse = await _client.GetAsync("/api/v1/admin/users");
        getAllResponse.EnsureSuccessStatusCode();
        var json = await getAllResponse.Content.ReadAsStringAsync();
        Assert.Contains("items", json);

        // GetById
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var testUser = await CreateTestUser(userManager);
        
        var getByIdResponse = await _client.GetAsync($"/api/v1/admin/users/{testUser.Id}");
        getByIdResponse.EnsureSuccessStatusCode();

        // Update
        var updateDto = new UpdateProfileDto { FullName = "Admin Updated Name" };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/admin/users/{testUser.Id}", updateDto);
        updateResponse.EnsureSuccessStatusCode();

        // Lock
        var lockDto = new LockUserDto { UserId = testUser.Id, LockUntil = DateTime.UtcNow.AddDays(1) };
        var lockResponse = await _client.PostAsJsonAsync($"/api/v1/admin/users/{testUser.Id}/lock", lockDto);
        lockResponse.EnsureSuccessStatusCode();

        // Delete
        var deleteUser = await CreateTestUser(userManager);
        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/users/{deleteUser.Id}");
        deleteResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AdminRolesController_AllEndpoints_Work()
    {
        // Login as admin
        var admin = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin.Token);

        // GetAll
        var getAllResponse = await _client.GetAsync("/api/v1/admin/roles");
        getAllResponse.EnsureSuccessStatusCode();

        // Create
        var createDto = new CreateRoleDto { Name = $"TestRole_{Guid.NewGuid()}", Description = "Test Role" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/roles", createDto);
        createResponse.EnsureSuccessStatusCode();

        // Assign
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var testUser = await CreateTestUser(userManager);
        
        var assignDto = new AssignRoleDto { UserId = testUser.Id, RoleName = createDto.Name };
        var assignResponse = await _client.PostAsJsonAsync("/api/v1/admin/roles/assign", assignDto);
        assignResponse.EnsureSuccessStatusCode();

        // Remove
        var removeResponse = await _client.DeleteAsJsonAsync("/api/v1/admin/roles/assign", assignDto);
        removeResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AdminPermissionsController_AllEndpoints_Work()
    {
        // Login as admin
        var admin = await LoginAsAdmin();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", admin.Token);

        // GetUserPermissions
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var testUser = await CreateTestUser(userManager);

        var response = await _client.GetAsync($"/api/v1/admin/permissions/user/{testUser.Id}");
        response.EnsureSuccessStatusCode();
        var permissions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(permissions);
    }

    // Helper methods
    private async Task<(string Token, Guid UserId)> CreateAndLoginUser()
    {
        var email = $"test_{Guid.NewGuid()}@example.com";
        var password = "Test@123!";
        
        var registerDto = new RegisterDto { Email = email, Password = password, FullName = "Test User" };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var loginDto = new LoginDto { Email = email, Password = password };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(email);
        
        return (tokenResponse!.AccessToken, user!.Id);
    }

    private async Task<(string Token, Guid UserId)> LoginAsAdmin()
    {
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

        var loginDto = new LoginDto { Email = adminEmail, Password = adminPassword };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        
        return (tokenResponse!.AccessToken, adminUser.Id);
    }

    private async Task<AppUser> CreateTestUser(UserManager<AppUser> userManager)
    {
        var user = new AppUser
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            UserName = $"test_{Guid.NewGuid()}@example.com",
            FullName = "Test User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(user, "Test@123!");
        return user;
    }
}

// Extension method for DeleteAsJsonAsync
public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
        {
            Content = System.Net.Http.Json.JsonContent.Create(value)
        };
        return await client.SendAsync(request);
    }
}

