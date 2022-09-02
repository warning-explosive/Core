namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Disposable
    /// </summary>
    public static class Disposable
    {
        /// <summary>
        /// Gets empty disposable object
        /// </summary>
        public static IDisposable Empty { get; } = new EmptyDisposable();

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="openScopeAction">openScopeAction</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <typeparam name="TState">TState</typeparam>
        /// <returns>IDisposable</returns>
        public static IDisposable Create<TState>(TState state, Action<TState> openScopeAction, Action<TState> finallyAction)
        {
            openScopeAction(state);
            return new DisposableAction<TState>(state, finallyAction);
        }

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <typeparam name="TState">TState</typeparam>
        /// <returns>IDisposable</returns>
        public static IDisposable Create<TState>(TState state, Action<TState> finallyAction)
        {
            return new DisposableAction<TState>(state, finallyAction);
        }

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="openScopeAction">openScopeAction</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static IDisposable Create(Action openScopeAction, Action finallyAction)
        {
            openScopeAction();
            return new DisposableAction(finallyAction);
        }

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static IDisposable Create(Action finallyAction)
        {
            return new DisposableAction(finallyAction);
        }

        /// <summary>
        /// Custom composite disposable (disposes like stack - LIFO)
        /// </summary>
        /// <param name="disposables">disposables</param>
        /// <returns>ICompositeDisposable (disposes like stack - LIFO)</returns>
        public static ICompositeDisposable CreateComposite(params IDisposable[] disposables)
        {
            return new CompositeDisposable(disposables);
        }

        private class CompositeDisposable : ICompositeDisposable
        {
            private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();

            public CompositeDisposable(params IDisposable[] disposables)
            {
                foreach (var disposable in disposables)
                {
                    _disposables.Push(disposable);
                }
            }

            public void Push(IDisposable disposable)
            {
                _disposables.Push(disposable);
            }

            public void Dispose()
            {
                _disposables.Each(d => d.Dispose());
                _disposables.Clear();
            }
        }

        private class DisposableAction<TState> : IDisposable
        {
            private readonly TState _state;
            private readonly Action<TState> _finallyAction;

            public DisposableAction(TState state, Action<TState> finallyAction)
            {
                _state = state;
                _finallyAction = finallyAction;
            }

            public void Dispose()
            {
                _finallyAction.Invoke(_state);
            }
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _finallyAction;

            public DisposableAction(Action finallyAction)
            {
                _finallyAction = finallyAction;
            }

            public void Dispose()
            {
                _finallyAction.Invoke();
            }
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}