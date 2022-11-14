namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// MessageQueue
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    [SuppressMessage("Analysis", "SA1124", Justification = "Readability")]
    public class MessageQueue<TElement> : IQueue<TElement>,
                                          IAsyncQueue<TElement>
    {
        private readonly Exclusive _exclusive = new Exclusive();

        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<TElement> _queue;

        /// <summary> .cctor </summary>
        public MessageQueue()
        {
            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<TElement>();
        }

        #region IQueue

        /// <inheritdoc />
        public int Count => _queue.Count;

        /// <inheritdoc />
        public bool IsEmpty => _queue.IsEmpty;

        /// <inheritdoc />
        public void Enqueue(TElement element)
        {
            _queue.Enqueue(element);
            _autoResetEvent.Set();
        }

        /// <inheritdoc />
        public TElement Dequeue()
        {
            throw new NotSupportedException(nameof(Dequeue));
        }

        /// <inheritdoc />
        public bool TryDequeue([NotNullWhen(true)] out TElement? element)
        {
            throw new NotSupportedException(nameof(TryDequeue));
        }

        /// <inheritdoc />
        public TElement Peek()
        {
            throw new NotSupportedException(nameof(Peek));
        }

        /// <inheritdoc />
        public bool TryPeek([NotNullWhen(true)] out TElement? element)
        {
            throw new NotSupportedException(nameof(TryPeek));
        }

        #endregion

        #region IAsyncQueue

        /// <inheritdoc />
        public Task Enqueue(TElement element, CancellationToken token)
        {
            Enqueue(element);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Run(Func<TElement, CancellationToken, Task> callback, CancellationToken token)
        {
            using (await _exclusive.Run(token).ConfigureAwait(false))
            {
                while (!token.IsCancellationRequested)
                {
                    var element = await Dequeue(token).ConfigureAwait(false);

                    if (element != null)
                    {
                        await callback(element, token).ConfigureAwait(false);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns first element and removes it from the MessageQueue.
        /// Waits while the MessageQueue is empty.
        /// Returns default value when cancellation was requested.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>First element in the MessageQueue</returns>
        private async Task<TElement?> Dequeue(CancellationToken token)
        {
            await _autoResetEvent.WaitAsync(token).ConfigureAwait(false);

            if (token.IsCancellationRequested)
            {
                return default;
            }

            return _queue.TryDequeue(out var element)
                ? element
                : throw new InvalidOperationException($"{nameof(MessageQueue<TElement>)} was corrupted");
        }
    }
}