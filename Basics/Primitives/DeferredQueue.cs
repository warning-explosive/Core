namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// DeferredQueue
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public class DeferredQueue<TElement> : IQueue<TElement>,
                                           IAsyncQueue<TElement>
        where TElement : IEquatable<TElement>,
                         IComparable<TElement>,
                         IComparable
    {
        private readonly TimeSpan _high = TimeSpan.FromMilliseconds(42);
        private readonly TimeSpan _low = TimeSpan.FromMilliseconds(1);

        private readonly IHeap<HeapEntry<TElement, DateTime>> _heap;
        private readonly PriorityQueue<TElement, DateTime> _priorityQueue;
        private readonly Func<TElement, DateTime> _prioritySelector;
        private readonly State _state;

        private Task? _delay;
        private CancellationTokenSource? _cts;

        /// <summary> .cctor </summary>
        /// <param name="heap">Heap implementation</param>
        /// <param name="prioritySelector">Priority selector</param>
        public DeferredQueue(
            IHeap<HeapEntry<TElement, DateTime>> heap,
            Func<TElement, DateTime> prioritySelector)
        {
            _heap = heap;
            _prioritySelector = prioritySelector;
            _priorityQueue = new PriorityQueue<TElement, DateTime>(heap, prioritySelector);
            _state = new State();
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (_priorityQueue)
                {
                    return _priorityQueue.Count;
                }
            }
        }

        /// <inheritdoc />
        public bool IsEmpty
        {
            get
            {
                lock (_priorityQueue)
                {
                    return _priorityQueue.IsEmpty;
                }
            }
        }

        /// <inheritdoc />
        public void Enqueue(TElement element)
        {
            lock (_priorityQueue)
            {
                _priorityQueue.Enqueue(element);
            }
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
        public async Task Run(Func<TElement, CancellationToken, Task> callback, CancellationToken token)
        {
            using (_state.StartExclusiveOperation())
            using (Disposable.Create(() => _heap.RootNodeChanged += CancelScheduleOnRootNodeChanged,
                () => _heap.RootNodeChanged -= CancelScheduleOnRootNodeChanged))
            {
                while (!token.IsCancellationRequested)
                {
                    (_delay, _cts) = Schedule(token);

                    try
                    {
                        await _delay.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        continue;
                    }

                    var args = DequeueSync();
                    var planned = _prioritySelector(args);

                    await WaitForBreadcrumbs(planned, token).ConfigureAwait(false);
                    await callback(args, token).ConfigureAwait(false);
                }
            }

            await CancelSchedule().ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            async void CancelScheduleOnRootNodeChanged(object sender, RootNodeChangedEventArgs<HeapEntry<TElement, DateTime>> args)
            {
                await CancelSchedule().ConfigureAwait(false);
            }
        }

        private TElement DequeueSync()
        {
            lock (_priorityQueue)
            {
                return _priorityQueue.Dequeue();
            }
        }

        private (Task, CancellationTokenSource?) Schedule(CancellationToken token)
        {
            lock (_priorityQueue)
            {
                _ = _priorityQueue.TryPeek(out var element);

                return element == null
                    ? InfiniteDelay(token)
                    : ElementDelay(element, token);
            }
        }

        private static (Task, CancellationTokenSource?) InfiniteDelay(CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var delay = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

            return (delay, cts);
        }

        private (Task, CancellationTokenSource?) ElementDelay(TElement element, CancellationToken token)
        {
            Task delay;
            CancellationTokenSource? cts;

            var now = DateTime.Now;
            var planned = _prioritySelector(element);

            if (planned <= now)
            {
                delay = Task.CompletedTask;
                cts = null;
            }
            else
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                delay = Task.Delay(planned - now, cts.Token);
            }

            return (delay, cts);
        }

        private async Task CancelSchedule()
        {
            var cts = Interlocked.Exchange(ref _cts, null);

            if (cts == null
                || typeof(CancellationTokenSource).GetFieldValue<bool>(cts, "_disposed"))
            {
                return;
            }

            using (cts)
            {
                cts.Cancel();

                try
                {
                    await _delay.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task WaitForBreadcrumbs(DateTime planned, CancellationToken token)
        {
            var now = DateTime.Now;

            if (planned <= now)
            {
                return;
            }

            var delta = planned - now;

            if (delta < _low)
            {
                return;
            }

            if (_low <= delta && delta <= _high)
            {
                await Task.Delay(delta, token).ConfigureAwait(false);
                await WaitForBreadcrumbs(planned, token).ConfigureAwait(false);
                return;
            }

            throw new InvalidOperationException($"Operation was started earlier than planned in {delta.TotalMilliseconds} ms: planned: {planned:O}; now: {now:O};");
        }
    }
}