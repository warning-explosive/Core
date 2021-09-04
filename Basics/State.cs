namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// State
    /// </summary>
    public class State
    {
        private readonly ConcurrentDictionary<string, object?> _state;
        private readonly ConcurrentDictionary<string, object> _sync;

        /// <summary> .cctor </summary>
        public State()
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
            var compositeKey = string.Join(typeof(TState).FullName, key);

            lock (_sync.GetOrAdd(compositeKey, _ => new object()))
            {
                var originalValue = _state.TryGetValue(key, out var value)
                                    && value is TState typed
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
            var compositeKey = string.Join(typeof(TState).FullName, key);

            lock (_sync.GetOrAdd(compositeKey, _ => new object()))
            {
                var originalValue = _state.TryGetValue(key, out var value)
                                    && value is TState typed
                    ? typed
                    : default;

                var replacement = producer(originalValue, context);

                _state[key] = replacement;

                return originalValue;
            }
        }
    }
}