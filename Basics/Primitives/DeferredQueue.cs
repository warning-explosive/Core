namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// DeferredQueue
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public class DeferredQueue<TElement> : IQueue<TElement>, IAsyncQueue<TElement>
        where TElement : IEquatable<TElement>, IComparable<TElement>, IComparable
    {
        private readonly Func<TElement, DateTime> _prioritySelector;
        private readonly PriorityQueue<TElement, DateTime> _priorityQueue;

        /// <summary> .cctor </summary>
        /// <param name="heap">Heap implementation</param>
        /// <param name="prioritySelector">Priority selector</param>
        public DeferredQueue(
            IHeap<HeapEntry<TElement, DateTime>> heap,
            Func<TElement, DateTime> prioritySelector)
        {
            _prioritySelector = prioritySelector;
            _priorityQueue = new PriorityQueue<TElement, DateTime>(heap, prioritySelector);
        }

        /// <inheritdoc />
        public int Count => _priorityQueue.Count;

        /// <inheritdoc />
        public bool IsEmpty => _priorityQueue.IsEmpty;

        /// <inheritdoc />
        public void Enqueue(TElement element)
        {
            _priorityQueue.Enqueue(element);
        }

        /// <inheritdoc />
        public TElement Dequeue()
        {
            return _priorityQueue.Dequeue();
        }

        /// <inheritdoc />
        public TElement Peek()
        {
            return _priorityQueue.Peek();
        }

        /// <inheritdoc />
        public async Task Run(Func<TElement, Task> callback, CancellationToken token)
        {
            /* TODO: TEST: 1 min -> wait 1 sec -> add with 1 sec -> wait 1 sec instead of 59 sec */

            var emptyQueueTimeout = TimeSpan.FromMilliseconds(100);
            var delay = Task.CompletedTask;

            while (!token.IsCancellationRequested)
            {
                await delay.ConfigureAwait(false);

                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (_priorityQueue.IsEmpty)
                {
                    delay = Task.Delay(emptyQueueTimeout, token);
                    continue;
                }

                var task = _priorityQueue.Dequeue();
                var planned = _prioritySelector(task);
                var now = DateTime.Now;

                if (planned <= now)
                {
                    await callback(task).WaitAsync(token).ConfigureAwait(false);
                }
                else
                {
                    delay = Task.Delay(planned - now, token);
                    _priorityQueue.Enqueue(task);
                }
            }
        }
    }
}