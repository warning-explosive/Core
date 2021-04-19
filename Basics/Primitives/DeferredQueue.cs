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
        private const string Scheduled = nameof(Scheduled);

        private readonly IHeap<HeapEntry<TElement, DateTime>> _heap;
        private readonly PriorityQueue<TElement, DateTime> _priorityQueue;
        private readonly Func<TElement, DateTime> _prioritySelector;
        private readonly State _state;

        /// <summary> .cctor </summary>
        /// <param name="heap">Heap implementation</param>
        /// <param name="prioritySelector">Priority selector</param>
        public DeferredQueue(
            IHeap<HeapEntry<TElement, DateTime>> heap,
            Func<TElement, DateTime> prioritySelector)
        {
            _heap = heap;
            _priorityQueue = new PriorityQueue<TElement, DateTime>(heap, prioritySelector);
            _prioritySelector = prioritySelector;

            _state = new State();
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
            CancellationTokenSource? cts = null;

            using (_state.StartExclusiveOperation())
            using (Disposable.Create(() => _heap.RootNodeChanged += LocalOnRootNodeChanged,
                () => _heap.RootNodeChanged -= LocalOnRootNodeChanged))
            {
                while (!token.IsCancellationRequested)
                {
                    var info = _state.Exchange<(Task, CancellationTokenSource?)>(Scheduled, _ => default);

                    if (info == default)
                    {
                        _priorityQueue.TryPeek(out var element);
                        info = ScheduleElement(element, token, out _);
                    }

                    Task delay;
                    (delay, cts) = info;

                    try
                    {
                        await delay.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        continue;
                    }

                    if (!_priorityQueue.TryDequeue(out var args))
                    {
                        continue;
                    }

                    var now = DateTime.Now;
                    var planned = _prioritySelector(args);

                    if (planned > now)
                    {
                        throw new InvalidOperationException($"Operation was started earlier than planned: planned: {planned:O}; now: {now:O};");
                    }

                    await callback(args).WaitAsync(token).ConfigureAwait(false);
                }
            }

            cts?.Cancel();
            cts?.Dispose();

            void LocalOnRootNodeChanged(object sender, RootNodeChangedEventArgs<HeapEntry<TElement, DateTime>> args)
            {
                var element = args.CurrentValue == default
                    ? default
                    : args.CurrentValue.Element;

                ScheduleElement(element, token, out var originalSchedule);

                var (_, originalCts) = originalSchedule;

                originalCts?.Cancel();
                originalCts?.Dispose();
            }
        }

        private (Task, CancellationTokenSource?) ScheduleElement(TElement? element, CancellationToken token, out (Task, CancellationTokenSource?) originalSchedule)
        {
            var info = element == null
                ? InfiniteDelay(token)
                : ElementDelay(element, token);

            originalSchedule = _state.Exchange<(Task, CancellationTokenSource? Cts)>(Scheduled, _ => info);

            return info;
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

            var planned = _prioritySelector(element);
            var now = DateTime.Now;

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
    }
}