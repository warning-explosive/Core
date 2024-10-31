namespace SpaceEngineers.Core.Basics;

using System;
using System.Threading;
using System.Threading.Tasks;
using SynchronizationPrimitives;

public class AsyncLazy<T>
{
    private readonly AsyncManualResetEvent _manualResetEvent;
    private readonly Func<CancellationToken, Task<T>> _producer;

    private T? _value;
    private int _produced;

    public AsyncLazy(Func<CancellationToken, Task<T>> producer)
    {
        _manualResetEvent = new AsyncManualResetEvent(false);
        _producer = producer;

        _value = default;
        _produced = 0;
    }

    public async Task<T> GetValue(CancellationToken? token = null)
    {
        token ??= CancellationToken.None;

        if (Interlocked.Exchange(ref _produced, 1) == 1)
        {
            await _manualResetEvent.WaitAsync(token.Value).ConfigureAwait(false);
        }
        else
        {
            _value = await _producer(token.Value).ConfigureAwait(false);
            _manualResetEvent.Set();
        }

        return _value !;
    }
}