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
}