namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncDisposableAction
    /// </summary>
    /// <typeparam name="TState">TState type-argument</typeparam>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct AsyncDisposableAction<TState> : IAsyncDisposable
    {
        private readonly TState _state;
        private readonly Func<TState, Task> _finallyAction;

        internal AsyncDisposableAction(TState state, Func<TState, Task> finallyAction)
        {
            _state = state;
            _finallyAction = finallyAction;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _finallyAction.Invoke(_state).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// AsyncDisposableAction
    /// </summary>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct AsyncDisposableAction : IAsyncDisposable
    {
        private readonly Func<Task> _finallyAction;

        internal AsyncDisposableAction(Func<Task> finallyAction)
        {
            _finallyAction = finallyAction;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _finallyAction.Invoke().ConfigureAwait(false);
        }
    }
}