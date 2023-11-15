namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// IHeap
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IHeap<TElement> : IEnumerable<TElement>
        where TElement : IEquatable<TElement>, IComparable<TElement>, IComparable
    {
        /// <summary>
        /// Root node changed
        /// </summary>
        event EventHandler<RootNodeChangedEventArgs<TElement>>? RootNodeChanged;

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
        /// Gets the highest-priority element but doesn't modify the heap
        /// </summary>
        /// <returns>The highest-priority element</returns>
        TElement Peek();

        /// <summary>
        /// Gets the highest-priority element but doesn't modify the heap if it isn't empty
        /// </summary>
        /// <param name="element">The highest-priority element or default value</param>
        /// <returns>Successful operation or not</returns>
        bool TryPeek([NotNullWhen(true)] out TElement? element);

        /// <summary>
        /// Gets the highest-priority element and removes it from the heap
        /// </summary>
        /// <returns>The highest-priority element</returns>
        TElement Extract();

        /// <summary>
        /// Gets the highest-priority element and removes it from the heap if it isn't empty
        /// </summary>
        /// <param name="element">The highest-priority element or default value</param>
        /// <returns>Successful operation or not</returns>
        bool TryExtract([NotNullWhen(true)] out TElement? element);

        /// <summary>
        /// Extract sorted array
        /// </summary>
        /// <returns>Array with priority ordering</returns>
        TElement[] ExtractArray();
    }
}