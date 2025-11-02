using DainnUserManagement.Application.Events;

namespace DainnUserManagement.Application.Interfaces;

/// <summary>
/// Interface for publishing domain events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a domain event asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the domain event.</typeparam>
    /// <param name="event">The domain event to publish.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
}

