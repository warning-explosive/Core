namespace SpaceEngineers.Core.Basics.SynchronizationPrimitives;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Free interpretation of CountdownEvent with several differences from original sync event
/// Differences:
/// - Signaled state is state when inner counter has reached zero or initialized with zero
/// - Increment resets event to non signaled state
/// - Decrement sets signaled state if inner counter has reached zero value
/// </summary>
public class AsyncCountdownEvent
{
    private readonly object _sync;
    private TaskCompletionSource<bool> _tcs;
    private int _count;

    public AsyncCountdownEvent(int count)
    {
        _sync = new object();
        _tcs = CreateCompletionSource<bool>();
        _count = count;

        if (_count <= 0)
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

    public int Increment()
    {
        lock (_sync)
        {
            if (_tcs.Task.IsCompleted)
            {
                _tcs = CreateCompletionSource<bool>();
            }

            return ++_count;
        }
    }

    public int Decrement()
    {
        lock (_sync)
        {
            var result = --_count;

            if (result <= 0)
            {
                _ = _tcs.TrySetResult(true);
            }

            return result;
        }
    }

    public int Read()
    {
        lock (_sync)
        {
            return _count;
        }
    }

    private static TaskCompletionSource<TResult> CreateCompletionSource<TResult>()
    {
        return new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}