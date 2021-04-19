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
    public class MessageQueue<TElement> : IQueue<TElement>,
                                          IAsyncQueue<TElement>
    {
        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<TElement> _queue;
        private readonly State _state;

        /// <summary> .cctor </summary>
        public MessageQueue()
        {
            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<TElement>();

            _state = new State();
        }

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

        /// <inheritdoc />
        public async Task Run(Func<TElement, Task> callback, CancellationToken token)
        {
            using (_state.StartExclusiveOperation())
            {
                while (!token.IsCancellationRequested)
                {
                    // TODO: dequeue several messages
                    var element = await Dequeue(token).ConfigureAwait(false);

                    if (element != null)
                    {
                        await callback(element).ConfigureAwait(false);
                    }
                }
            }
        }

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