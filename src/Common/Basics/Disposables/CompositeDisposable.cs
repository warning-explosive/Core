namespace SpaceEngineers.Core.Basics.Disposables;

using System;
using System.Collections.Generic;

public readonly struct CompositeDisposable : IDisposable
{
    private readonly Stack<IDisposable> _disposables = new();

    internal CompositeDisposable(params IDisposable[] disposables)
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