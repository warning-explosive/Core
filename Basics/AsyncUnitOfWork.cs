namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class AsyncUnitOfWork<TContext> : IAsyncUnitOfWork<TContext>
    {
        private readonly CancellationToken _token;
        private readonly Func<TContext, CancellationToken, Task> _onCommitAction;
        private readonly Func<TContext, CancellationToken, Task> _onRollbackAction;

        private int _saveChanges;

        internal AsyncUnitOfWork(
            TContext context,
            Func<TContext, CancellationToken, Task> onCommitAction,
            Func<TContext, CancellationToken, Task> onRollbackAction,
            CancellationToken token)
        {
            Context = context;
            _onCommitAction = onCommitAction;
            _onRollbackAction = onRollbackAction;
            _token = token;
        }

        public TContext Context { get; }

        public void SaveChanges()
        {
            if (Interlocked.Exchange(ref _saveChanges, 1) != default)
            {
                throw new InvalidOperationException("You already mark this logical transaction as committed");
            }
        }

        public ValueTask DisposeAsync()
        {
            var operation = Interlocked.CompareExchange(ref _saveChanges, 0, 0) == default
                ? Rollback()
                : Commit();

            return new ValueTask(operation);
        }

        private Task Rollback()
        {
            return _onRollbackAction.Invoke(Context, _token);
        }

        private Task Commit()
        {
            return _onCommitAction.Invoke(Context, _token);
        }
    }
}