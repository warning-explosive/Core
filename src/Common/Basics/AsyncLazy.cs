namespace SpaceEngineers.Core.Basics;

using System;
using System.Threading;
using System.Threading.Tasks;
using SynchronizationPrimitives;

public class AsyncLazy<T>(Func<CancellationToken, Task<T>> producer)
{
    private readonly AsyncManualResetEvent _manualResetEvent = new(false);

    private T? _value;
    private int _produced;

    public async Task<T> GetValue(CancellationToken? token = null)
    {
        token ??= CancellationToken.None;

        if (Interlocked.Exchange(ref _produced, 1) == 1)
        {
            await _manualResetEvent.WaitAsync(token.Value).ConfigureAwait(false);
        }
        else
        {
            _value = await producer(token.Value).ConfigureAwait(false);
            _manualResetEvent.Set();
        }

        return _value !;
    }
}