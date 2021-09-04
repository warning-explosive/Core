namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Async lazy structure
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public class AsyncLazy<T>
    {
        private readonly AsyncManualResetEvent _manualResetEvent;
        private readonly Func<CancellationToken, Task<T>> _producer;

        private T? _value;
        private int _produced;

        /// <summary> .cctor </summary>
        /// <param name="producer">Value producer</param>
        public AsyncLazy(Func<CancellationToken, Task<T>> producer)
        {
            _manualResetEvent = new AsyncManualResetEvent(false);
            _producer = producer;

            _value = default;
            _produced = 0;
        }

        /// <summary>
        /// Gets value in lazy and asynchronous manner
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing lazy calculation</returns>
        public async Task<T> GetValue(CancellationToken? token = null)
        {
            token ??= CancellationToken.None;

            if (Interlocked.Exchange(ref _produced, 1) == 1)
            {
                await _manualResetEvent.WaitAsync(token.Value).ConfigureAwait(false);
            }
            else
            {
                _value = await _producer(token.Value).ConfigureAwait(false);
                _manualResetEvent.Set();
            }

            return _value !;
        }
    }
}