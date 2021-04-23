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
            using (Disposable.Create(() => _heap.RootNodeChanged += ScheduleOnRootNodeChanged,
                () => _heap.RootNodeChanged -= ScheduleOnRootNodeChanged))
            {
                while (!token.IsCancellationRequested)
                {
                    Task delay;
                    (delay, cts) = ScheduleNext(token);

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

            void ScheduleOnRootNodeChanged(object sender, RootNodeChangedEventArgs<HeapEntry<TElement, DateTime>> args)
            {
                var element = args.CurrentValue == default
                    ? default
                    : args.CurrentValue.Element;

                ReScheduleNext(element, token);
            }
        }

        private (Task, CancellationTokenSource?) ScheduleNext(CancellationToken token)
        {
            (Task, CancellationTokenSource?) schedule = default;

            /* TODO: wrong lock order -> dead lock -> 1. lock _state -> 2. lock heap */

            _ = _state.Exchange<(Task, CancellationTokenSource?), CancellationToken>(
                Scheduled,
                token,
                (original, cancellationToken) =>
                {
                    if (original != default)
                    {
                        schedule = original;
                    }
                    else
                    {
                        _priorityQueue.TryPeek(out var element);
                        schedule = ScheduleElement(element, cancellationToken);
                    }

                    return schedule;
                });

            return schedule;
        }

        private void ReScheduleNext(TElement? element, CancellationToken token)
        {
            /* TODO: wrong lock order -> dead lock -> 1. lock heap -> 2. lock _state */

            var originalSchedule = _state.Exchange<(Task, CancellationTokenSource?), (TElement? Element, CancellationToken Token)>(
                Scheduled,
                (element, token),
                (_, context) => ScheduleElement(context.Element, context.Token));

            var (_, originalCts) = originalSchedule;

            originalCts?.Cancel();
            originalCts?.Dispose();
        }

        private (Task, CancellationTokenSource?) ScheduleElement(TElement? element, CancellationToken token)
        {
            return element == null
                ? InfiniteDelay(token)
                : ElementDelay(element, token);
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