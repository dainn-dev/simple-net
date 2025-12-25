using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DainnUser.PostgreSQL.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DainnUserManagement.Tests.Integration;

/// <summary>
/// Factory for creating test web applications.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing database context registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
            });

            // Remove any existing IEmailService implementations
            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DainnUser.PostgreSQL.Application.Interfaces.IEmailService));
            if (emailDescriptor != null)
            {
                services.Remove(emailDescriptor);
            }

            // Register mock email service
            services.AddScoped<DainnUser.PostgreSQL.Application.Interfaces.IEmailService, MockEmailService>();

            // Remove existing event handlers
            var handlerDescriptors = services.Where(
                d => d.ServiceType.IsGenericType &&
                d.ServiceType.GetGenericTypeDefinition() == typeof(DainnUser.PostgreSQL.Application.Interfaces.IEventHandler<>)).ToList();
            foreach (var handler in handlerDescriptors)
            {
                services.Remove(handler);
            }

            // Register mock event handlers
            services.AddScoped<DainnUser.PostgreSQL.Application.Interfaces.IEventHandler<DainnUser.PostgreSQL.Application.Events.UserRegisteredEvent>, MockWelcomeEmailHandler>();

            // Configure logging
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }
}

/// <summary>
/// Mock email service for testing.
/// </summary>
public class MockEmailService : DainnUser.PostgreSQL.Application.Interfaces.IEmailService
{
    public List<string> SentEmails { get; } = new();
    public List<(string Email, string Token)> ConfirmationEmails { get; } = new();
    public List<(string Email, string Token)> ResetEmails { get; } = new();

    public Task SendConfirmationEmailAsync(string email, string confirmationToken)
    {
        SentEmails.Add(email);
        ConfirmationEmails.Add((email, confirmationToken));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetToken)
    {
        SentEmails.Add(email);
        ResetEmails.Add((email, resetToken));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Mock event handler for testing welcome emails.
/// </summary>
public class MockWelcomeEmailHandler : DainnUser.PostgreSQL.Application.Interfaces.IEventHandler<DainnUser.PostgreSQL.Application.Events.UserRegisteredEvent>
{
    public List<DainnUser.PostgreSQL.Application.Events.UserRegisteredEvent> HandledEvents { get; } = new();

    public Task HandleAsync(DainnUser.PostgreSQL.Application.Events.UserRegisteredEvent @event)
    {
        HandledEvents.Add(@event);
        return Task.CompletedTask;
    }
}

