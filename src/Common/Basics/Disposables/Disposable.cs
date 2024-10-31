namespace SpaceEngineers.Core.Basics.Disposables;

using System;

public static class Disposable
{
    public static EmptyDisposable Empty { get; } = default;

    public static DisposableAction<TState> Create<TState>(TState state, Action<TState> openScopeAction, Action<TState> finallyAction)
    {
        openScopeAction(state);
        return new DisposableAction<TState>(state, finallyAction);
    }

    public static DisposableAction<TState> Create<TState>(TState state, Action<TState> finallyAction)
    {
        return new DisposableAction<TState>(state, finallyAction);
    }

    public static DisposableAction Create(Action openScopeAction, Action finallyAction)
    {
        openScopeAction();
        return new DisposableAction(finallyAction);
    }

    public static DisposableAction Create(Action finallyAction)
    {
        return new DisposableAction(finallyAction);
    }

    public static CompositeDisposable CreateComposite(params IDisposable[] disposables)
    {
        return new CompositeDisposable(disposables);
    }
}