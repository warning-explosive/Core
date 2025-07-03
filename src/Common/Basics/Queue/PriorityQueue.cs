namespace SpaceEngineers.Core.Basics.Queue;

using System;
using System.Diagnostics.CodeAnalysis;
using Heap;

public class PriorityQueue<TElement, TKey>(IHeap<HeapEntry<TElement, TKey>> heap, Func<TElement, TKey> prioritySelector) : IQueue<TElement>
    where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
{
    public int Count => heap.Count;

    public bool IsEmpty => heap.IsEmpty;

    public void Enqueue(TElement element)
    {
        var entry = new HeapEntry<TElement, TKey>(prioritySelector(element), element);
        heap.Insert(entry);
    }

    public TElement Dequeue()
    {
        return heap.Extract().Element;
    }

    public bool TryDequeue([NotNullWhen(true)] out TElement? element)
    {
        if (heap.TryExtract(out var entry))
        {
            element = entry.Element!;
            return true;
        }

        element = default;
        return false;
    }

    public TElement Peek()
    {
        return heap.Peek().Element;
    }

    public bool TryPeek([NotNullWhen(true)] out TElement? element)
    {
        if (heap.TryPeek(out var entry))
        {
            element = entry.Element!;
            return true;
        }

        element = default;
        return false;
    }
}