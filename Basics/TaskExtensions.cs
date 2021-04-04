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
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Task wrapped in cancellation callback</returns>
        public static Task WaitAsync(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return task;
            }

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : WaitAsyncInternal(task, cancellationToken);

            static async Task WaitAsyncInternal(Task task, CancellationToken cancellationToken)
            {
                using var cancelCompletionSource = new TaskCancellationCompletionSource<object>(cancellationToken);

                await Task.WhenAny(task, cancelCompletionSource.Task).ConfigureAwait(false);
            }
        }
    }
}