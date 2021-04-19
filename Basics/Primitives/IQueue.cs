namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// IQueue abstraction
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IQueue<TElement>
    {
        /// <summary>
        /// Gets the number of elements contained in the queue
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value that indicates whether the queue is empty
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds an element to the queue
        /// </summary>
        /// <param name="element">Element</param>
        void Enqueue(TElement element);

        /// <summary>
        /// Returns the first element and removes it from the queue
        /// </summary>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        TElement Dequeue();

        /// <summary>
        /// Returns the first element and removes it from the queue if queue is not empty
        /// </summary>
        /// <param name="element">The first element of default value</param>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        bool TryDequeue([NotNullWhen(true)] out TElement? element);

        /// <summary>
        /// Returns the first element but doesn't modify the queue
        /// </summary>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        TElement Peek();

        /// <summary>
        /// Returns the first element but doesn't modify the queue if queue is not empty
        /// </summary>
        /// <param name="element">The first element of default value</param>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        bool TryPeek([NotNullWhen(true)] out TElement? element);
    }
}