namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    public abstract class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
    {
        private int _started;

        /// <inheritdoc />
        public async Task StartTransaction(
            TContext context,
            Func<TContext, CancellationToken, Task> producer,
            bool saveChanges,
            CancellationToken token)
        {
            var startError = await ExecutionExtensions
                .TryAsync(() => StartTransactionUnsafe(context, producer, token))
                .Catch<Exception>()
                .Invoke(Task.FromResult<Exception?>)
                .ConfigureAwait(false);

            var finishError = await ExecutionExtensions
                .TryAsync(() => FinishTransactionUnsafe(context, saveChanges, startError, token))
                .Catch<Exception>()
                .Invoke(Task.FromResult<Exception?>)
                .ConfigureAwait(false);

            var exception = startError ?? finishError;

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Runs on start operations
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing on start operations</returns>
        protected virtual Task Start(TContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Rollback logical transaction
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="exception">Producers exception (optional)</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing rollback operation</returns>
        protected virtual Task Rollback(TContext context, Exception? exception, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Commit logical transaction
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing commit operation</returns>
        protected virtual Task Commit(TContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task<Exception?> StartTransactionUnsafe(TContext context, Func<TContext, CancellationToken, Task> producer, CancellationToken token)
        {
            if (Interlocked.Exchange(ref _started, 1) != default)
            {
                throw new InvalidOperationException("You have already started this logical transaction");
            }

            await Start(context, token).ConfigureAwait(false);

            await producer.Invoke(context, token).ConfigureAwait(false);

            return null;
        }

        private async Task<Exception?> FinishTransactionUnsafe(TContext context, bool saveChanges, Exception? exception, CancellationToken token)
        {
            var finishOperation = saveChanges && exception == null
                ? Commit(context, token)
                : Rollback(context, exception, token);

            await finishOperation.ConfigureAwait(false);

            return null;
        }
    }
}