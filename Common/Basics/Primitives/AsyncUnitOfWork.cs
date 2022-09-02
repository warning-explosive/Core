namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Enumerations;

    /// <inheritdoc />
    public abstract class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
    {
        private readonly SyncState _syncState = new SyncState();

        /// <inheritdoc />
        public async Task ExecuteInTransaction(
            TContext context,
            Func<TContext, CancellationToken, Task> producer,
            bool saveChanges,
            CancellationToken token)
        {
            using (_syncState.StartExclusiveOperation(nameof(AsyncUnitOfWork<TContext>)))
            {
                var (behavior, startError) = await ExecutionExtensions
                    .TryAsync(context, StartTransactionUnsafe)
                    .Catch<Exception>()
                    .Invoke(StartExceptionResult, token)
                    .ConfigureAwait(false);

                if (startError != null)
                {
                    throw startError.Rethrow();
                }

                if (behavior is EnUnitOfWorkBehavior.DoNotRun)
                {
                    return;
                }

                Exception? executionError = default;

                if (behavior is not EnUnitOfWorkBehavior.SkipProducer)
                {
                    executionError = await ExecutionExtensions
                        .TryAsync((context, producer), ExecuteProducerUnsafe)
                        .Catch<Exception>()
                        .Invoke(ExceptionResult, token)
                        .ConfigureAwait(false);
                }

                var finishError = await ExecutionExtensions
                    .TryAsync((context, saveChanges, executionError), FinishTransactionUnsafe)
                    .Catch<Exception>()
                    .Invoke(ExceptionResult, token)
                    .ConfigureAwait(false);

                var error = executionError ?? finishError;

                if (error != null)
                {
                    throw error.Rethrow();
                }
            }
        }

        /// <summary>
        /// Runs on start operations
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operations</returns>
        protected virtual Task<EnUnitOfWorkBehavior> Start(TContext context, CancellationToken token)
        {
            return Task.FromResult(EnUnitOfWorkBehavior.Regular);
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

        private async Task<(EnUnitOfWorkBehavior, Exception?)> StartTransactionUnsafe(TContext context, CancellationToken token)
        {
            var behavior = await Start(context, token).ConfigureAwait(false);
            return (behavior, null);
        }

        private static async Task<Exception?> ExecuteProducerUnsafe((TContext, Func<TContext, CancellationToken, Task>) state, CancellationToken token)
        {
            var (context, producer) = state;

            await producer.Invoke(context, token).ConfigureAwait(false);

            return null;
        }

        private async Task<Exception?> FinishTransactionUnsafe((TContext, bool, Exception?) state, CancellationToken token)
        {
            var (context, saveChanges, exception) = state;

            var finishOperation = saveChanges && exception == null
                ? Commit(context, token)
                : Rollback(context, exception, token);

            await finishOperation.ConfigureAwait(false);

            return null;
        }

        private static Task<(EnUnitOfWorkBehavior, Exception?)> StartExceptionResult(Exception exception, CancellationToken token)
        {
            return Task.FromResult((EnUnitOfWorkBehavior.DoNotRun, (Exception?)exception));
        }

        private static Task<Exception?> ExceptionResult(Exception exception, CancellationToken token)
        {
            return Task.FromResult<Exception?>(exception);
        }
    }
}