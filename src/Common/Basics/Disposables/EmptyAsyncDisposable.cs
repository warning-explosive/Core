namespace SpaceEngineers.Core.Basics.Disposables;

using System;
using System.Threading.Tasks;

public readonly struct EmptyAsyncDisposable : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}