namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;

    /// <summary>
    /// PriorityQueue
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public class PriorityQueue<TElement, TKey> : IQueue<TElement>
        where TKey : IEquatable<TKey>, IComparable<TKey>, IComparable
    {
        private readonly IHeap<HeapEntry<TElement, TKey>> _heap;
        private readonly Func<TElement, TKey> _prioritySelector;

        /// <summary> .cctor </summary>
        /// <param name="heap">Heap implementation</param>
        /// <param name="prioritySelector">Priority selector</param>
        public PriorityQueue(IHeap<HeapEntry<TElement, TKey>> heap, Func<TElement, TKey> prioritySelector)
        {
            _heap = heap;
            _prioritySelector = prioritySelector;
        }

        /// <inheritdoc />
        public int Count => _heap.Count;

        /// <inheritdoc />
        public bool IsEmpty => _heap.IsEmpty;

        /// <inheritdoc />
        public void Enqueue(TElement element)
        {
            var entry = new HeapEntry<TElement, TKey>(_prioritySelector(element), element);
            _heap.Insert(entry);
        }

        /// <inheritdoc />
        public TElement Dequeue()
        {
            return _heap.Extract().Element;
        }

        /// <inheritdoc />
        public TElement Peek()
        {
            return _heap.Peek().Element;
        }
    }
}