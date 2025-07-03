namespace SpaceEngineers.Core.Basics.SynchronizationPrimitives;

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class AsyncAutoResetEvent
{
    private static readonly TaskCompletionSource<bool> CompletedSource
        = CreateCompletedCompletionSource(true);

    private readonly ConcurrentQueue<TaskCompletionSource<bool>> _waits = new();

    private int _completed;

    public AsyncAutoResetEvent(bool isSet)
    {
        if (isSet)
        {
            Interlocked.Increment(ref _completed);
        }
    }

    public Task WaitAsync(CancellationToken? cancellationToken = null)
    {
        if (_completed > 0)
        {
            Interlocked.Decrement(ref _completed);
            return CompletedSource.Task;
        }

        var tcs = CreateCompletionSource<bool>();
        _waits.Enqueue(tcs);

        return cancellationToken != null
            ? tcs.Task.WaitAsync(cancellationToken.Value)
            : tcs.Task;
    }

    public void Set()
    {
        if (_waits.TryDequeue(out var toRelease))
        {
            _ = toRelease.TrySetResult(true);
        }
        else
        {
            Interlocked.Increment(ref _completed);
        }
    }

    private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
    {
        return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private static TaskCompletionSource<TResult> CreateCompletedCompletionSource<TResult>(TResult result)
    {
        var tcs = CreateCompletionSource<TResult>();
        _ = tcs.TrySetResult(result);
        return tcs;
    }
}