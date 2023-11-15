namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// TaskCancellationCompletionSource
    /// </summary>
    /// <typeparam name="TResult">TResult type-argument</typeparam>
    public class TaskCancellationCompletionSource<TResult> : TaskCompletionSource<TResult>, IDisposable
    {
        private readonly IDisposable? _registration;

        /// <summary> .cctor </summary>
        /// <param name="token">Cancellation token</param>
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

        /// <inheritdoc />
        public void Dispose()
        {
            _registration?.Dispose();
        }
    }
}