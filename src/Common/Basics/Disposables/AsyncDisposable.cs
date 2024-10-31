namespace SpaceEngineers.Core.Basics.Disposables;

using System;
using System.Threading.Tasks;

public static class AsyncDisposable
{
    public static EmptyAsyncDisposable Empty { get; } = default;

    public static AsyncDisposableAction Create(Func<Task> finallyAction)
    {
        return new AsyncDisposableAction(finallyAction.Invoke);
    }

    public static AsyncDisposableAction<TState> Create<TState>(TState state, Func<TState, Task> finallyAction)
    {
        return new AsyncDisposableAction<TState>(state, finallyAction);
    }
}