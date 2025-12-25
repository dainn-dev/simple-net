using DainnUser.PostgreSQL.Application.Events;

namespace DainnUser.PostgreSQL.Application.Interfaces;

/// <summary>
/// Interface for handling domain events.
/// </summary>
/// <typeparam name="T">The type of domain event to handle.</typeparam>
public interface IEventHandler<T> where T : IDomainEvent
{
    /// <summary>
    /// Handles a domain event asynchronously.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(T @event);
}

