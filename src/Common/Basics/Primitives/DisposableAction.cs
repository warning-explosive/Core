namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// DisposableAction
    /// </summary>
    /// <typeparam name="TState">TState type-argument</typeparam>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct DisposableAction<TState> : IDisposable
    {
        private readonly TState _state;
        private readonly Action<TState> _finallyAction;

        internal DisposableAction(TState state, Action<TState> finallyAction)
        {
            _state = state;
            _finallyAction = finallyAction;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _finallyAction.Invoke(_state);
        }
    }

    /// <summary>
    /// DisposableAction
    /// </summary>
    [SuppressMessage("Analysis", "CA1815", Justification = "unnecessary equality")]
    public struct DisposableAction : IDisposable
    {
        private readonly Action _finallyAction;

        internal DisposableAction(Action finallyAction)
        {
            _finallyAction = finallyAction;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _finallyAction.Invoke();
        }
    }
}