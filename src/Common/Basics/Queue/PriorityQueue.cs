namespace SpaceEngineers.Core.Basics.Queue;

using System;
using System.Diagnostics.CodeAnalysis;
using Heap;

public class PriorityQueue<TElement, TKey> : IQueue<TElement>
    where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
{
    private readonly IHeap<HeapEntry<TElement, TKey>> _heap;
    private readonly Func<TElement, TKey> _prioritySelector;

    public PriorityQueue(IHeap<HeapEntry<TElement, TKey>> heap, Func<TElement, TKey> prioritySelector)
    {
        _heap = heap;
        _prioritySelector = prioritySelector;
    }

    public int Count => _heap.Count;

    public bool IsEmpty => _heap.IsEmpty;

    public void Enqueue(TElement element)
    {
        var entry = new HeapEntry<TElement, TKey>(_prioritySelector(element), element);
        _heap.Insert(entry);
    }

    public TElement Dequeue()
    {
        return _heap.Extract().Element;
    }

    public bool TryDequeue([NotNullWhen(true)] out TElement? element)
    {
        if (_heap.TryExtract(out var entry))
        {
            element = entry.Element!;
            return true;
        }

        element = default;
        return false;
    }

    public TElement Peek()
    {
        return _heap.Peek().Element;
    }

    public bool TryPeek([NotNullWhen(true)] out TElement? element)
    {
        if (_heap.TryPeek(out var entry))
        {
            element = entry.Element!;
            return true;
        }

        element = default;
        return false;
    }
}