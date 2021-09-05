namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    public abstract class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
    {
        private readonly State _state = new State();

        /// <inheritdoc />
        public async Task ExecuteInTransaction(
            TContext context,
            Func<TContext, CancellationToken, Task> producer,
            bool saveChanges,
            CancellationToken token)
        {
            using (_state.StartExclusiveOperation())
            {
                var startError = await ExecutionExtensions
                    .TryAsync((context, producer), StartTransactionUnsafe)
                    .Catch<Exception>()
                    .Invoke(ExceptionResult, token)
                    .ConfigureAwait(false);

                var finishError = await ExecutionExtensions
                    .TryAsync((context, saveChanges, startError), FinishTransactionUnsafe)
                    .Catch<Exception>()
                    .Invoke(ExceptionResult, token)
                    .ConfigureAwait(false);

                var exception = startError ?? finishError;

                if (exception != null)
                {
                    throw exception.Rethrow();
                }
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

        private async Task<Exception?> StartTransactionUnsafe((TContext, Func<TContext, CancellationToken, Task>) state, CancellationToken token)
        {
            var (context, producer) = state;

            await Start(context, token).ConfigureAwait(false);

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

        private static Task<Exception?> ExceptionResult(Exception exception, CancellationToken token)
        {
            return Task.FromResult<Exception?>(exception);
        }
    }
}