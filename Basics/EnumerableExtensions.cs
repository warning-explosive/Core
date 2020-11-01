namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;

    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="leftSource">Left source</param>
        /// <param name="rightSource">Right source</param>
        /// <param name="leftKeySelector">Left key selector</param>
        /// <param name="rightKeySelector">Right key selector</param>
        /// <param name="resultSelector">Result selector (right could be null for reference types)</param>
        /// <typeparam name="TLeft">TLeft type-argument</typeparam>
        /// <typeparam name="TRight">TRight type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Left join result</returns>
        public static IEnumerable<TResult> LeftJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> leftSource,
            IEnumerable<TRight> rightSource,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft, TRight, TResult> resultSelector)
        {
            return from left in leftSource
                   join right in rightSource
                       on leftKeySelector(left) equals rightKeySelector(right)
                       into rightMatch
                   from nullableRight in rightMatch.DefaultIfEmpty()
                   select resultSelector(left, nullableRight);
        }

        /// <summary>
        /// Flatten source stream
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <param name="unfold">Unfold function</param>
        /// <typeparam name="T">Element type-argument</typeparam>
        /// <returns> Flatten source </returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> unfold)
        {
            return source.SelectMany(item => Flatten(unfold(item), unfold).Concat(new[] { item }));
        }

        /// <summary> Produce cartesian product of source columns </summary>
        /// <param name="sourceColumns">Source columns</param>
        /// <typeparam name="T">Item type-argument</typeparam>
        /// <returns>Cartesian product of source columns</returns>
        public static IEnumerable<ICollection<T>> ColumnsCartesianProduct<T>(this IEnumerable<IEnumerable<T>> sourceColumns)
        {
            if (!sourceColumns.Any())
            {
                return Enumerable.Empty<ICollection<T>>();
            }

            IEnumerable<ICollection<T>> seed = sourceColumns.Take(1)
                                                            .Single()
                                                            .Select(item => new List<T> { item });

            return sourceColumns.Skip(1)
                                .Aggregate(seed,
                                           (accumulator, next) =>
                                               accumulator.Join(next,
                                                                _ => true,
                                                                _ => true,
                                                                (left, right) => new List<T>(left) { right }));
        }

        /// <summary>
        /// Source enumerator without nulls
        /// </summary>
        /// <param name="source">Source enumerator</param>
        /// <typeparam name="TSource">Source item type type-argument</typeparam>
        /// <typeparam name="TReturn">TReturn source item type type-argument</typeparam>
        /// <returns>Source without nulls</returns>
        public static IEnumerable<TReturn> WithoutNulls<TSource, TReturn>(this IEnumerable<TSource> source)
            where TReturn : notnull, TSource
        {
            return source.Where(item => item != null)
                         .OfType<TReturn>();
        }

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

        /// <summary> Select collection from untyped IEnumerable </summary>
        /// <param name="enumerable">IEnumerable</param>
        /// <returns>Collection of objects</returns>
        public static IEnumerable<object> ToObjectEnumerable(this IEnumerable enumerable)
        {
            return enumerable.GetEnumerator().ToObjectEnumerable();
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
            source.Each(queue.Enqueue);

            return queue;
        }

        /// <summary>
        /// Informative single extraction
        /// </summary>
        /// <param name="source">Source collection</param>
        /// <param name="amb">Ambiguous message factory</param>
        /// <typeparam name="T">Collection item type-argument</typeparam>
        /// <returns>Single item with informative errors</returns>
        /// <exception cref="NotFoundException">Throws if source is empty</exception>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static T InformativeSingle<T>(this IEnumerable<T> source, Func<IEnumerable<T>, string> amb)
        {
            if (!source.Any())
            {
                throw new NotFoundException("Source collection is empty");
            }

            if (source.Take(2).Count() != 1)
            {
                throw new AmbiguousMatchException(amb(source));
            }

            return source.Single();
        }

        /// <summary>
        /// Informative single or default extraction
        /// </summary>
        /// <param name="source">Source collection</param>
        /// <param name="amb">Ambiguous message factory</param>
        /// <typeparam name="T">Collection item type-argument</typeparam>
        /// <returns>Single item with informative errors</returns>
        /// <exception cref="AmbiguousMatchException">Throws if source contains more than one element</exception>
        public static T InformativeSingleOrDefault<T>(this IEnumerable<T> source, Func<IEnumerable<T>, string> amb)
        {
            if (source.Take(2).Count() == 2)
            {
                throw new AmbiguousMatchException(amb(source));
            }

            return source.SingleOrDefault();
        }
    }
}