namespace SpaceEngineers.Core.Basics.Disposables;

using System;

public struct DisposableAction<TState> : IDisposable
{
    private readonly TState _state;
    private readonly Action<TState> _finallyAction;

    internal DisposableAction(TState state, Action<TState> finallyAction)
    {
        _state = state;
        _finallyAction = finallyAction;
    }

    public void Dispose()
    {
        _finallyAction.Invoke(_state);
    }
}

public struct DisposableAction : IDisposable
{
    private readonly Action _finallyAction;

    internal DisposableAction(Action finallyAction)
    {
        _finallyAction = finallyAction;
    }

    public void Dispose()
    {
        _finallyAction.Invoke();
    }
}