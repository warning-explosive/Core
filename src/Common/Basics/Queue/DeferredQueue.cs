namespace SpaceEngineers.Core.Basics.Queue;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Disposables;
using Heap;
using SynchronizationPrimitives;

public class DeferredQueue<TElement>(
    IHeap<HeapEntry<TElement, DateTime>> heap,
    Func<TElement, DateTime> prioritySelector)
    : IQueue<TElement>, IAsyncQueue<TElement>
    where TElement : IEquatable<TElement>, IComparable<TElement>, IComparable
{
    private readonly Exclusive _exclusive = new();

    private readonly TimeSpan _high = TimeSpan.FromMilliseconds(42);
    private readonly TimeSpan _low = TimeSpan.FromMilliseconds(1);

    private readonly PriorityQueue<TElement, DateTime> _priorityQueue = new(heap, prioritySelector);

    private Task? _delay;
    private CancellationTokenSource? _cts;

    #region IQueue

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

    public void Enqueue(TElement element)
    {
        lock (_priorityQueue)
        {
            _priorityQueue.Enqueue(element);
        }
    }

    public TElement Dequeue()
    {
        throw new NotSupportedException(nameof(Dequeue));
    }

    public bool TryDequeue([NotNullWhen(true)] out TElement? element)
    {
        throw new NotSupportedException(nameof(TryDequeue));
    }

    public TElement Peek()
    {
        throw new NotSupportedException(nameof(Peek));
    }

    public bool TryPeek([NotNullWhen(true)] out TElement? element)
    {
        throw new NotSupportedException(nameof(TryPeek));
    }

    #endregion

    #region IAsyncQueue

    public Task Enqueue(TElement element, CancellationToken token)
    {
        Enqueue(element);
        return Task.CompletedTask;
    }

    public async Task Run(Func<TElement, CancellationToken, Task> callback, CancellationToken token)
    {
        using (await _exclusive.Run(token).ConfigureAwait(false))
        using (Disposable.Create(
                   new EventHandler<RootNodeChangedEventArgs<HeapEntry<TElement, DateTime>>>(CancelScheduleOnRootNodeChanged),
                   subscription => heap.RootNodeChanged += subscription,
                   subscription => heap.RootNodeChanged -= subscription))
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
                var planned = prioritySelector(args);

                try
                {
                    await WaitForBreadcrumbs(planned, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }

                await callback(args, token).ConfigureAwait(false);
            }
        }

        await CancelSchedule().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();

        async void CancelScheduleOnRootNodeChanged(object? sender, RootNodeChangedEventArgs<HeapEntry<TElement, DateTime>> args)
        {
            await CancelSchedule().ConfigureAwait(false);
        }
    }

    #endregion

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

        var now = DateTime.UtcNow;
        var planned = prioritySelector(element).ToUniversalTime();

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
                if (_delay != null)
                {
                    await _delay.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    private async Task WaitForBreadcrumbs(DateTime planned, CancellationToken token)
    {
        var now = DateTime.UtcNow;

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