using System;

namespace Core.Events
{
    /// <summary>
    /// Type-safe synchronous pub/sub for gameplay events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Registers a handler for events of type <typeparamref name="T"/>.
        /// Multiple handlers may subscribe to the same event type.
        /// </summary>
        /// <typeparam name="T">The event type; must implement <see cref="IEvent"/>.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        void Subscribe<T>(Action<T> handler) where T : IEvent;

        /// <summary>
        /// Removes a previously subscribed handler. No-op if the handler was never subscribed.
        /// Removes the type entry when the last handler is removed.
        /// </summary>
        /// <typeparam name="T">The event type; must implement <see cref="IEvent"/>.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        void Unsubscribe<T>(Action<T> handler) where T : IEvent;

        /// <summary>
        /// Invokes all handlers for <typeparamref name="T"/> synchronously.
        /// </summary>
        /// <typeparam name="T">The event type; must implement <see cref="IEvent"/>.</typeparam>
        /// <remarks>
        /// If a subscriber throws, remaining subscribers for this event are not invoked;
        /// the exception propagates to the caller.
        /// </remarks>
        void Publish<T>(T evt) where T : IEvent;
    }
}
