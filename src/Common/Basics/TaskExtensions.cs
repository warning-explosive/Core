namespace SpaceEngineers.Core.Basics;

using System.Threading;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static async Task WaitAsync(this Task task, CancellationToken token)
    {
        if (!token.CanBeCanceled)
        {
            await task.ConfigureAwait(false);
            return;
        }

        using (var tcs = new TaskCancellationCompletionSource<object?>(token))
        {
            await Task
                .WhenAny(task, tcs.Task)
                .Unwrap()
                .ConfigureAwait(false);
        }
    }
}