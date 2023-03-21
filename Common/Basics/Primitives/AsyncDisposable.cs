namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Async disposable
    /// </summary>
    public static class AsyncDisposable
    {
        /// <summary>
        /// Empty async disposable
        /// </summary>
        [SuppressMessage("Analysis", "SA1129", Justification = "explicit details")]
        public static EmptyAsyncDisposable Empty { get; } = new EmptyAsyncDisposable();

        /// <summary>
        /// Creates IAsyncDisposable
        /// </summary>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static AsyncDisposableAction Create(Func<Task> finallyAction)
        {
            return new AsyncDisposableAction(finallyAction.Invoke);
        }

        /// <summary>
        /// Creates IAsyncDisposable
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <typeparam name="TState">TState</typeparam>
        /// <returns>IDisposable</returns>
        public static AsyncDisposableAction<TState> Create<TState>(TState state, Func<TState, Task> finallyAction)
        {
            return new AsyncDisposableAction<TState>(state, finallyAction);
        }
    }
}