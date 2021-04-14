namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;

    /// <summary>
    /// IQueue abstraction
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public interface IQueue<TElement>
    {
        /// <summary>
        /// Gets the number of elements contained in the queue
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a value that indicates whether the queue is empty
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Adds an element to the queue
        /// </summary>
        /// <param name="element">Element</param>
        public void Enqueue(TElement element);

        /// <summary>
        /// Returns the first element and removes it from the queue
        /// </summary>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        public TElement Dequeue();

        /// <summary>
        /// Returns the first element but doesn't modify the queue
        /// </summary>
        /// <returns>The first element</returns>
        /// <exception cref="InvalidOperationException">Queue is empty</exception>
        public TElement Peek();
    }
}