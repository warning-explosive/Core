namespace SpaceEngineers.Core.Basics.Disposables;

using System;
using System.Threading.Tasks;

public readonly struct AsyncDisposableAction<TState> : IAsyncDisposable
{
    private readonly TState _state;
    private readonly Func<TState, Task> _finallyAction;

    internal AsyncDisposableAction(TState state, Func<TState, Task> finallyAction)
    {
        _state = state;
        _finallyAction = finallyAction;
    }

    public async ValueTask DisposeAsync()
    {
        await _finallyAction.Invoke(_state).ConfigureAwait(false);
    }
}

public readonly struct AsyncDisposableAction : IAsyncDisposable
{
    private readonly Func<Task> _finallyAction;

    internal AsyncDisposableAction(Func<Task> finallyAction)
    {
        _finallyAction = finallyAction;
    }

    public async ValueTask DisposeAsync()
    {
        await _finallyAction.Invoke().ConfigureAwait(false);
    }
}