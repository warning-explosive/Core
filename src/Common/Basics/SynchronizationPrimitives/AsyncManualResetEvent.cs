namespace SpaceEngineers.Core.Basics.SynchronizationPrimitives;

using System.Threading;
using System.Threading.Tasks;

public class AsyncManualResetEvent
{
    private readonly object _sync;

    private TaskCompletionSource<bool> _tcs;

    public AsyncManualResetEvent(bool isSet)
    {
        _sync = new object();

        _tcs = CreateCompletionSource<bool>();

        if (isSet)
        {
            _tcs.TrySetResult(true);
        }
    }

    public Task WaitAsync(CancellationToken? cancellationToken = null)
    {
        Task waitTask;

        lock (_sync)
        {
            waitTask = _tcs.Task;
        }

        return waitTask.IsCompleted
               || cancellationToken == null
            ? waitTask
            : waitTask.WaitAsync(cancellationToken.Value);
    }

    public void Set()
    {
        lock (_sync)
        {
            _tcs.TrySetResult(true);
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            if (_tcs.Task.IsCompleted)
            {
                _tcs = CreateCompletionSource<bool>();
            }
        }
    }

    private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
    {
        return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}