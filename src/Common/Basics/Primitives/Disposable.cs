namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Disposable
    /// </summary>
    public static class Disposable
    {
        /// <summary>
        /// Gets empty disposable object
        /// </summary>
        [SuppressMessage("Analysis", "SA1129", Justification = "explicit details")]
        public static EmptyDisposable Empty { get; } = new EmptyDisposable();

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="state">state</param>
        /// <param name="openScopeAction">openScopeAction</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <typeparam name="TState">TState</typeparam>
        /// <returns>IDisposable</returns>
        public static DisposableAction<TState> Create<TState>(TState state, Action<TState> openScopeAction, Action<TState> finallyAction)
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
        public static DisposableAction<TState> Create<TState>(TState state, Action<TState> finallyAction)
        {
            return new DisposableAction<TState>(state, finallyAction);
        }

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="openScopeAction">openScopeAction</param>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static DisposableAction Create(Action openScopeAction, Action finallyAction)
        {
            openScopeAction();
            return new DisposableAction(finallyAction);
        }

        /// <summary>
        /// Creates disposable object
        /// </summary>
        /// <param name="finallyAction">finallyAction</param>
        /// <returns>IDisposable</returns>
        public static DisposableAction Create(Action finallyAction)
        {
            return new DisposableAction(finallyAction);
        }

        /// <summary>
        /// Custom composite disposable (disposes like stack - LIFO)
        /// </summary>
        /// <param name="disposables">disposables</param>
        /// <returns>ICompositeDisposable (disposes like stack - LIFO)</returns>
        public static CompositeDisposable CreateComposite(params IDisposable[] disposables)
        {
            return new CompositeDisposable(disposables);
        }
    }
}