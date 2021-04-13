namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IHeap
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IHeap<TElement> : IEnumerable<TElement>
        where TElement : IComparable<TElement>
    {
        /// <summary>
        /// Gets the number of elements contained in the heap
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value that indicates whether the heap is empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds an element to the heap
        /// </summary>
        /// <param name="element">Element</param>
        void Insert(TElement element);

        /// <summary>
        /// Gets the highest-priority element and removes it from the heap
        /// </summary>
        /// <returns>The highest-priority element</returns>
        TElement Extract();

        /// <summary>
        /// Extract sorted array
        /// </summary>
        /// <returns>Array with priority ordering</returns>
        TElement[] ExtractArray();
    }
}