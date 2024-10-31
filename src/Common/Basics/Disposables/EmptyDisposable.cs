namespace SpaceEngineers.Core.Basics.Disposables;

using System;

public struct EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}