namespace SpaceEngineers.Core.Basics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Primitives;

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
                using var cancelCompletionSource = new TaskCancellationCompletionSource<object>(token);

                await Task.WhenAny(task, cancelCompletionSource.Task).ConfigureAwait(false);
            }
        }
    }
}