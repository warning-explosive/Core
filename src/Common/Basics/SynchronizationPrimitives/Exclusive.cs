namespace SpaceEngineers.Core.Basics.SynchronizationPrimitives;

using System;
using System.Threading;
using System.Threading.Tasks;
using Disposables;

public class Exclusive
{
    private readonly AsyncAutoResetEvent _sync;

    private bool _isTaken;

    public Exclusive()
    {
        _sync = new AsyncAutoResetEvent(true);
        _isTaken = false;
    }

    public async Task<IDisposable> Run(CancellationToken token)
    {
        if (_isTaken)
        {
            throw new InvalidOperationException("Exclusive operation has already been started");
        }

        await _sync
            .WaitAsync(token)
            .ConfigureAwait(false);

        _isTaken = true;

        return Disposable.Create(this, Finally);

        static void Finally(Exclusive exclusive)
        {
            exclusive._sync.Set();
            exclusive._isTaken = false;
        }
    }
}