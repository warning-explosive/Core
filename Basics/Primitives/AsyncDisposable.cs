namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Async disposable
    /// </summary>
    public static class AsyncDisposable
    {
        /// <summary>
        /// Empty async disposable
        /// </summary>
        public static IAsyncDisposable Empty { get; } = new EmptyAsyncDisposable();

        /// <summary>
        /// Creates IAsyncDisposable
        /// </summary>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static IAsyncDisposable Create(Func<Task> finallyAction)
        {
            return new AsyncDisposableAction<object?>(null, _ => finallyAction.Invoke());
        }

        /// <summary>
        /// Creates IAsyncDisposable
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <typeparam name="TState">TState</typeparam>
        /// <returns>IDisposable</returns>
        public static IAsyncDisposable Create<TState>(TState state, Func<TState, Task> finallyAction)
        {
            return new AsyncDisposableAction<TState>(state, finallyAction);
        }

        private class AsyncDisposableAction<TState> : IAsyncDisposable
        {
            private readonly TState _state;
            private readonly Func<TState, Task> _finallyAction;

            public AsyncDisposableAction(TState state, Func<TState, Task> finallyAction)
            {
                _state = state;
                _finallyAction = finallyAction;
            }

            public async ValueTask DisposeAsync()
            {
                await _finallyAction.Invoke(_state).ConfigureAwait(false);
            }
        }

        private class EmptyAsyncDisposable : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                await Task.CompletedTask.ConfigureAwait(false);
            }
        }
    }
}