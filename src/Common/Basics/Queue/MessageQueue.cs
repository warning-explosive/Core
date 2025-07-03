namespace SpaceEngineers.Core.Basics.Queue;

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SynchronizationPrimitives;

public class MessageQueue<TElement> : IQueue<TElement>,
    IAsyncQueue<TElement>
{
    private readonly Exclusive _exclusive = new();

    private readonly AsyncAutoResetEvent _autoResetEvent = new(false);
    private readonly ConcurrentQueue<TElement> _queue = new();

    #region IQueue

    public int Count => _queue.Count;

    public bool IsEmpty => _queue.IsEmpty;

    public void Enqueue(TElement element)
    {
        _queue.Enqueue(element);
        _autoResetEvent.Set();
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

    private async Task<TElement?> Dequeue(CancellationToken token)
    {
        /*
         * Waits while the MessageQueue is empty.
         * Returns default value when cancellation was requested.
         */

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