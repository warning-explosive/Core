namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary> Execute action on each element </summary>
        /// <param name="source">A sequence of values to invoke an action on</param>
        /// <param name="action">An action to apply to each source element</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" /></typeparam>
        public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary> Execute action on each element with index </summary>
        /// <param name="source">A sequence of values to invoke an action on</param>
        /// <param name="action">An action to apply to each source element</param>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source" /></typeparam>
        public static void Each<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
        {
            var length = source.Count();

            for (var i = 0; i < length; ++i)
            {
                action(source.ElementAt(i), i);
            }
        }

        /// <summary> Select collection from IEnumerator </summary>
        /// <param name="numerator">IEnumerator</param>
        /// <returns>Collection of objects</returns>
        public static IEnumerable<object> ToObjectEnumerable(this IEnumerator numerator)
        {
            while (numerator.MoveNext())
            {
                yield return numerator.Current;
            }
        }

        /// <summary>
        /// Enqueue ordered collection into queue instance
        /// First the queue is cleared
        /// </summary>
        /// <param name="queue">Target queue</param>
        /// <param name="source">Ordered collection</param>
        /// <typeparam name="T">Item type-argument</typeparam>
        /// <returns>Filled queue</returns>
        public static Queue<T> EnqueueMany<T>(this Queue<T> queue, IReadOnlyCollection<T> source)
        {
            queue.Clear();
            source.Each(queue.Enqueue);

            return queue;
        }
    }
}