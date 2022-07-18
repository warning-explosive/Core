namespace SpaceEngineers.Core.Basics
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Task class extensions
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Wait task asynchronously with cancellation
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="token">Optional cancellation token</param>
        /// <returns>Task wrapped in cancellation callback</returns>
        public static Task WaitAsync(this Task task, CancellationToken token)
        {
            if (!token.CanBeCanceled)
            {
                return task;
            }

            return token.IsCancellationRequested
                ? Task.FromCanceled(token)
                : WaitAsyncInternal(task, token);

            static async Task WaitAsyncInternal(Task task, CancellationToken token)
            {
                var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

                if (token.IsCancellationRequested)
                {
                    _ = tcs.TrySetCanceled();
                    return;
                }

                using (token.Register(() => tcs.TrySetCanceled(token), useSynchronizationContext: false))
                {
                    await Task
                       .WhenAny(task, tcs.Task)
                       .Unwrap()
                       .ConfigureAwait(false);
                }
            }
        }
    }
}