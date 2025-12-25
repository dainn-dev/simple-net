using DainnUser.PostgreSQL.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DainnUser.PostgreSQL.Application.Events;

/// <summary>
/// Publisher for domain events that resolves and invokes event handlers.
/// </summary>
public class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public EventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Publishes a domain event asynchronously by resolving and invoking all registered handlers.
    /// </summary>
    /// <typeparam name="T">The type of the domain event.</typeparam>
    /// <param name="event">The domain event to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual async Task PublishAsync<T>(T @event) where T : IDomainEvent
    {
        // Resolve all handlers for this event type
        var handlers = _serviceProvider.GetServices<IEventHandler<T>>();

        // Invoke all handlers concurrently
        var tasks = handlers.Select(handler => handler.HandleAsync(@event));
        await Task.WhenAll(tasks);
    }
}

