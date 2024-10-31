namespace SpaceEngineers.Core.Basics;

using System;
using System.Threading;
using System.Threading.Tasks;

public class TaskCancellationCompletionSource<TResult> : TaskCompletionSource<TResult>, IDisposable
{
    private readonly IDisposable? _registration;

    public TaskCancellationCompletionSource(CancellationToken token)
        : base(TaskCreationOptions.RunContinuationsAsynchronously)
    {
        if (!token.CanBeCanceled)
        {
            throw new InvalidOperationException("Cancellation token can't be in cancelled state");
        }

        if (token.IsCancellationRequested)
        {
            _ = TrySetCanceled();
            return;
        }

        _registration = token.Register(() => TrySetCanceled(token), useSynchronizationContext: false);
    }

    public void Dispose()
    {
        _registration?.Dispose();
    }
}