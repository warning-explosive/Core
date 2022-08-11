namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// SyncState
    /// </summary>
    public class SyncState
    {
        private readonly ConcurrentDictionary<string, object?> _state;
        private readonly ConcurrentDictionary<string, object> _sync;

        /// <summary> .cctor </summary>
        public SyncState()
        {
            _state = new ConcurrentDictionary<string, object?>();
            _sync = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// Atomically changes state value
        /// </summary>
        /// <param name="key">State key</param>
        /// <param name="producer">Value producer</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <returns>Original state value</returns>
        public TState? Exchange<TState>(string key, Func<TState?, TState> producer)
        {
            lock (_sync.GetOrAdd(key, _ => new object()))
            {
                var originalValue = _state.TryGetValue(key, out var value) && value is TState typed
                    ? typed
                    : default;

                var replacement = producer(originalValue);

                _state[key] = replacement;

                return originalValue;
            }
        }

        /// <summary>
        /// Atomically changes state value
        /// </summary>
        /// <param name="key">State key</param>
        /// <param name="context">Additional context</param>
        /// <param name="producer">Value producer</param>
        /// <typeparam name="TState">TState type-argument</typeparam>
        /// <typeparam name="TContext">TContext type-argument</typeparam>
        /// <returns>Original state value</returns>
        public TState? Exchange<TState, TContext>(string key, TContext context, Func<TState?, TContext, TState> producer)
        {
            lock (_sync.GetOrAdd(key, _ => new object()))
            {
                var originalValue = _state.TryGetValue(key, out var value) && value is TState typed
                    ? typed
                    : default;

                var replacement = producer(originalValue, context);

                _state[key] = replacement;

                return originalValue;
            }
        }
    }
}