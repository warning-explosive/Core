namespace SpaceEngineers.Core.Basics.Primitives
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
        /// <param name="token">Cancellation token</param>
        public TaskCancellationCompletionSource(CancellationToken token)
        {
            _tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (token.IsCancellationRequested)
            {
                _tcs.SetCanceled();
                return;
            }

            _registration = token
                .Register(() => _tcs.TrySetCanceled(token), useSynchronizationContext: false);
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