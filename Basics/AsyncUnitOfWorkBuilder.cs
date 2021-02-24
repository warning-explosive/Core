namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncUnitOfWorkBuilder
    /// </summary>
    /// <typeparam name="TContext">TContext type-argument</typeparam>
    public class AsyncUnitOfWorkBuilder<TContext>
    {
        private static readonly Func<TContext, CancellationToken, Task> Empty = (ctx, token) => Task.CompletedTask;

        private Func<TContext, CancellationToken, Task> _onCommitFunction = Empty;
        private Func<TContext, CancellationToken, Task> _onRollbackFunction = Empty;

        /// <summary>
        /// Open logical transaction
        /// </summary>
        /// <param name="context">Context data container</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Opened logical transaction</returns>
        public IAsyncUnitOfWork<TContext> OpenTransaction(TContext context, CancellationToken token)
        {
            return new AsyncUnitOfWork<TContext>(context, _onCommitFunction, _onRollbackFunction, token);
        }

        /// <summary>
        /// Register callback that fires when transaction is committed
        /// </summary>
        /// <param name="onCommitFunction">On commit function</param>
        /// <returns>Logical transaction builder</returns>
        public AsyncUnitOfWorkBuilder<TContext> RegisterOnCommit(Func<TContext, CancellationToken, Task> onCommitFunction)
        {
            _onCommitFunction = onCommitFunction;
            return this;
        }

        /// <summary>
        /// Register callback that fires when transaction is rolled back
        /// </summary>
        /// <param name="onRollbackFunction">On rollback function</param>
        /// <returns>Logical transaction builder</returns>
        public AsyncUnitOfWorkBuilder<TContext> RegisterOnRollback(Func<TContext, CancellationToken, Task> onRollbackFunction)
        {
            _onRollbackFunction = onRollbackFunction;
            return this;
        }
    }
}