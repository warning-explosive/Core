namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Async disposable
    /// </summary>
    public static class AsyncDisposable
    {
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

            public ValueTask DisposeAsync()
            {
                return new ValueTask(_finallyAction.Invoke(_state));
            }
        }
    }
}