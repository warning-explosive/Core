namespace SpaceEngineers.Core.Basics.Async
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TaskCancellationCompletionSource
    /// </summary>
    /// <typeparam name="TResult">TResult type-argument</typeparam>
    public class TaskCancellationCompletionSource<TResult> : IDisposable
    {
        private readonly TaskCompletionSource<TResult> _tcs;
        private readonly IDisposable? _registration;

        /// <summary> .cctor </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        public TaskCancellationCompletionSource(CancellationToken cancellationToken)
        {
            _tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (cancellationToken.IsCancellationRequested)
            {
                _tcs.SetCanceled();
                return;
            }

            _registration = cancellationToken
                .Register(() => _tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
        }

        /// <summary>
        /// Task
        /// </summary>
        public Task<TResult> Task => _tcs.Task;

        /// <inheritdoc />
        public void Dispose()
        {
            _registration?.Dispose();
        }
    }
}