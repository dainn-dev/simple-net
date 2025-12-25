using System.Net;
using System.Net.Http.Json;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Application.Events;
using Microsoft.Extensions.DependencyInjection;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Integration tests for event flow.
/// Tests: Register → Welcome email handler called
/// </summary>
public class EventFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EventFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_PublishesUserRegisteredEvent_CallsEventHandler()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"eventtest_{Guid.NewGuid()}@example.com",
            Password = "Test@123!",
            FullName = "Event Test User"
        };

        using var scope = _factory.Services.CreateScope();
        var eventHandler = scope.ServiceProvider.GetRequiredService<MockWelcomeEmailHandler>();
        eventHandler.HandledEvents.Clear();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Wait a bit for async event processing
        await Task.Delay(100);
        
        // Verify event handler was called
        Assert.True(eventHandler.HandledEvents.Count > 0, "Event handler should have been called");
        var handledEvent = eventHandler.HandledEvents.First();
        Assert.Equal(registerDto.Email, handledEvent.Email);
        Assert.Equal(registerDto.FullName, handledEvent.FullName);
        Assert.NotEqual(Guid.Empty, handledEvent.UserId);
    }

    [Fact]
    public async Task Register_SendsConfirmationEmail()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = $"emailtest_{Guid.NewGuid()}@example.com",
            Password = "Test@123!",
            FullName = "Email Test User"
        };

        using var scope = _factory.Services.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<MockEmailService>();
        emailService.SentEmails.Clear();
        emailService.ConfirmationEmails.Clear();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Wait a bit for async processing
        await Task.Delay(100);
        
        // Verify email service was called
        Assert.True(emailService.SentEmails.Contains(registerDto.Email), "Confirmation email should have been sent");
        Assert.True(emailService.ConfirmationEmails.Any(e => e.Email == registerDto.Email), "Confirmation email should be in the list");
    }
}

