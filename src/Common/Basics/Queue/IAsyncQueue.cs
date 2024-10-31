namespace SpaceEngineers.Core.Basics.Queue;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IAsyncQueue<TElement>
{
    Task Enqueue(TElement element, CancellationToken token);

    Task Run(Func<TElement, CancellationToken, Task> callback, CancellationToken token);
}