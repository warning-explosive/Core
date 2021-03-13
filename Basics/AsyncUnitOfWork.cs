namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc />
    public abstract class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
    {
        private TContext? _context;
        private CancellationTokenSource? _cts;

        private int _saveChanges;
        private int _started;
        private int _disposed;

        /// <inheritdoc />
        public TContext Context => _context.EnsureNotNull<TContext>("You should start transaction before");

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        /// <inheritdoc />
        public void SaveChanges()
        {
            if (Interlocked.CompareExchange(ref _started, 0, 0) == default)
            {
                throw new InvalidOperationException("You should start transaction before");
            }

            if (Interlocked.Exchange(ref _saveChanges, 1) != default)
            {
                throw new InvalidOperationException("You have already marked this logical transaction as committed");
            }
        }

        /// <inheritdoc />
        public async Task<IAsyncDisposable> StartTransaction(TContext context, CancellationToken token)
        {
            _context = context;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            if (Interlocked.Exchange(ref _started, 1) != default)
            {
                throw new InvalidOperationException("You have already started this logical transaction");
            }

            await Start(context, Token).ConfigureAwait(false);

            return AsyncDisposable.Create(this, unitOfWork => unitOfWork.DisposeAsync());
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
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing rollback operation</returns>
        protected virtual Task Rollback(TContext context, CancellationToken token)
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

        private async Task DisposeAsync()
        {
            if (Interlocked.Exchange(ref _started, default) == default)
            {
                return;
            }

            if (Interlocked.Exchange(ref _disposed, 1) != default)
            {
                throw new InvalidOperationException("You have already disposed this logical transaction");
            }

            var operation = Interlocked.CompareExchange(ref _saveChanges, default, default) == default
                ? Rollback(Context, Token)
                : Commit(Context, Token);

            await operation.ConfigureAwait(false);

            _cts?.Dispose();
        }
    }
}