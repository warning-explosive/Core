namespace SpaceEngineers.Core.Basics;

using System.Collections.Generic;

public static class QueueExtensions
{
    public static Queue<T> EnqueueMany<T>(this Queue<T> queue, IReadOnlyCollection<T> source)
    {
        source.Each(queue.Enqueue);

        return queue;
    }
}