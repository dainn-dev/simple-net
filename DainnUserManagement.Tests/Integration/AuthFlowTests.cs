using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DainnUserManagement.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DainnUserManagement.Domain.Entities;
using DainnUserManagement.Infrastructure.Persistence;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Integration tests for E2E authentication flow.
/// Tests: Register → Confirm → Login → Refresh → 2FA → Access protected
/// </summary>
public class AuthFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task E2E_AuthFlow_Register_Confirm_Login_Refresh_2FA_AccessProtected()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test@123!",
            FullName = "Test User"
        };

        // Act & Assert: Register
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();
        var registeredUser = await registerResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(registeredUser);
        Assert.Equal(registerDto.Email, registeredUser.Email);
        Assert.True(registeredUser.CreatedAt > DateTime.MinValue);

        // Get confirmation token from database
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync(registerDto.Email);
        Assert.NotNull(user);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmDto = new ConfirmEmailDto
        {
            UserId = user.Id,
            Token = token
        };

        // Act & Assert: Confirm Email
        var confirmResponse = await _client.PostAsJsonAsync("/api/v1/email/confirm", confirmDto);
        confirmResponse.EnsureSuccessStatusCode();

        // Act & Assert: Login
        var loginDto = new LoginDto
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();
        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(tokenResponse);
        Assert.NotNull(tokenResponse.AccessToken);
        Assert.NotNull(tokenResponse.RefreshToken);

        // Act & Assert: Access Protected Endpoint
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
        var meResponse = await _client.GetAsync("/api/v1/account/me");
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(me);
        Assert.Equal(registerDto.Email, me.Email);

        // Act & Assert: Refresh Token
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = tokenResponse.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshDto);
        refreshResponse.EnsureSuccessStatusCode();
        var newTokenResponse = await refreshResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Assert.NotNull(newTokenResponse);
        Assert.NotNull(newTokenResponse.AccessToken);
        Assert.NotEqual(tokenResponse.AccessToken, newTokenResponse.AccessToken);

        // Act & Assert: Setup 2FA
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newTokenResponse.AccessToken);
        var setup2FAResponse = await _client.PostAsync("/api/v1/account/2fa/setup", null);
        setup2FAResponse.EnsureSuccessStatusCode();
        var setup2FA = await setup2FAResponse.Content.ReadFromJsonAsync<TwoFactorSetupDto>();
        Assert.NotNull(setup2FA);
        Assert.NotNull(setup2FA.Secret);
        Assert.NotNull(setup2FA.QrCodeUrl);

        // Verify 2FA code (would need to generate actual TOTP code in a real test)
        // For now, we just verify the endpoint exists and returns proper structure
        Assert.True(setup2FA.Secret.Length > 0);
        Assert.True(setup2FA.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Register_ReturnsUserProfile_WithCorrectData()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test@123!",
            FullName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(user);
        Assert.Equal(registerDto.Email, user.Email);
        Assert.Equal(registerDto.FullName, user.FullName);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.NotNull(user.CreatedAt);

        // Verify JSON shape
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"email\"", json);
        Assert.Contains("\"fullName\"", json);
        Assert.Contains("\"id\"", json);
    }
}

