namespace SpaceEngineers.Core.Basics.Disposables;

using System;

public readonly struct EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}