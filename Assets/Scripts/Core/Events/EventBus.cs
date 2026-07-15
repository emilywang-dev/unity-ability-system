using System;
using System.Collections.Generic;

namespace Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/> — sync multicast, no GetInvocationList in Publish.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        /// <inheritdoc/>
        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) 
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var key = typeof(T);
            if (_handlers.TryGetValue(key, out var existing))
            {
                _handlers[key] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[key] = handler;
            }
        }

        /// <inheritdoc/>
        public void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) 
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var key = typeof(T);
            if (!_handlers.TryGetValue(key, out var existing))
            {
                return;
            }

            var updated = Delegate.Remove(existing, handler);
            if (updated == null)
            {
                _handlers.Remove(key);
            }
            else
            {
                _handlers[key] = updated;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Internal handler storage is corrupt.</exception>
        public void Publish<T>(T evt) where T : IEvent
        {
            if (!_handlers.TryGetValue(typeof(T), out var del))
            {
                return;
            }

            // Internal invariant violation if cast fails — not a caller error.
            if (del is not Action<T> action)
            {
                throw new InvalidOperationException(
                    $"Internal EventBus error: handler for {typeof(T).Name} is not Action<{typeof(T).Name}>.");
            }

            // No per-subscriber exception isolation.
            action(evt);
        }
    }
}
