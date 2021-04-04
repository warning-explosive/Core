namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncTaskQueue
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    [SuppressMessage("Analysis", "CA1711", Justification = "Async primitive")]
    public class AsyncTaskQueue<TElement>
    {
        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<TElement> _queue;

        /// <summary> .cctor </summary>
        public AsyncTaskQueue()
        {
            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<TElement>();
        }

        /// <summary>
        /// Gets the number of elements contained in the AsyncTaskQueue
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Gets a value that indicates whether the AsyncTaskQueue is empty
        /// </summary>
        public bool IsEmpty => _queue.IsEmpty;

        /// <summary>
        /// Adds an element to the AsyncTaskQueue
        /// </summary>
        /// <param name="element">Element</param>
        public void Enqueue(TElement element)
        {
            _queue.Enqueue(element);
            _autoResetEvent.Set();
        }

        /// <summary>
        /// Returns first element and removes it from the AsyncTaskQueue.
        /// Waits while the AsyncTaskQueue is empty.
        /// Returns default value in case of the cancellation.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>First element in the AsyncTaskQueue</returns>
        public async Task<TElement?> Dequeue(CancellationToken token)
        {
            await _autoResetEvent.WaitAsync(token).ConfigureAwait(false);

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return _queue.TryDequeue(out var element)
                ? element
                : throw new InvalidOperationException($"{nameof(AsyncTaskQueue<TElement>)} was corrupted");
        }
    }
}