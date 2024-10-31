namespace SpaceEngineers.Core.Basics.Queue;

using System.Diagnostics.CodeAnalysis;

public interface IQueue<TElement>
{
    int Count { get; }

    bool IsEmpty { get; }

    void Enqueue(TElement element);

    TElement Dequeue();

    bool TryDequeue([NotNullWhen(true)] out TElement? element);

    TElement Peek();

    bool TryPeek([NotNullWhen(true)] out TElement? element);
}