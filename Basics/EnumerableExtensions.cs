namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;
    using Primitives;

    /// <summary>
    /// Enumerable extensions
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Distinct by specified key selector
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="comparer">Custom equality comparer</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Distinct source</returns>
        public static IEnumerable<TValue> DistinctBy<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            return source
               .GroupBy(keySelector, comparer)
               .Select(it => it.First());
        }

        /// <summary>
        /// Stacks source collection into separate piles defined by key selector
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        /// <returns>Separate piles</returns>
        public static IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> Stack<TKey, TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, TKey> keySelector)
        {
            return source
                .Aggregate(new Stack<KeyValuePair<TKey, List<TValue>>>(), Aggregate)
                .Reverse()
                .Select(pair => new KeyValuePair<TKey, IEnumerable<TValue>>(pair.Key, pair.Value));

            Stack<KeyValuePair<TKey, List<TValue>>> Aggregate(Stack<KeyValuePair<TKey, List<TValue>>> acc, TValue next)
            {
                var key = keySelector(next);

                if (!acc.TryPeek(out var peek)
                    || !EqualityComparer<TKey>.Default.Equals(key, peek.Key))
                {
                    acc.Push(new KeyValuePair<TKey, List<TValue>>(key, new List<TValue> { next }));
                }
                else
                {
                    peek.Value.Add(next);
                }

                return acc;
            }
        }

        /// <summary>
        /// Full outer join
        /// </summary>
        /// <param name="leftSource">Left source</param>
        /// <param name="rightSource">Right source</param>
        /// <param name="leftKeySelector">Left key selector</param>
        /// <param name="rightKeySelector">Right key selector</param>
        /// <param name="resultSelector">Result selector (left/right could be null for reference types)</param>
        /// <param name="comparer">Custom equality comparer</param>
        /// <typeparam name="TLeft">TLeft type-argument</typeparam>
        /// <typeparam name="TRight">TRight type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Full outer join result</returns>
        public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> leftSource,
            IEnumerable<TRight> rightSource,
            Func<TLeft, TKey> leftKeySelector,
            Func<TRight, TKey> rightKeySelector,
            Func<TLeft?, TRight?, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            var leftLookup = leftSource.ToLookup(leftKeySelector);
            var rightLookup = rightSource.ToLookup(rightKeySelector);

            var keys = leftLookup
                .Select(p => p.Key)
                .Concat(rightLookup.Select(p => p.Key))
                .ToHashSet(comparer ?? EqualityComparer<TKey>.Default);

            return from key in keys
                from left in leftLookup[key].DefaultIfEmpty()
                from right in rightLookup[key].DefaultIfEmpty()
                select resultSelector(left, right);
        }

        /// <summary>
        /// Left outer join
        /// </summary>
        /// <param name="leftSource">Left source</param>
        /// <param name="rightSource">Right source</param>
        /// <param name="leftKeySelector">Left key selector</param>
        /// <param name="rightKeySelector">Right key selector</param>
        /// <param name="resultSelector">Result selector (right could be null for reference types)</param>
        /// <param name="comparer">Custom equality comparer</param>
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
            Func<TLeft, TRight?, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer = null)
        {
            var leftLookup = leftSource.ToLookup(leftKeySelector);
            var rightLookup = rightSource.ToLookup(rightKeySelector);

            var keys = leftLookup
                .Select(p => p.Key)
                .ToHashSet(comparer ?? EqualityComparer<TKey>.Default);

            return from key in keys
                from left in leftLookup[key]
                from right in rightLookup[key].DefaultIfEmpty()
                select resultSelector(left, right);
        }

        /// <summary>
        /// Flatten source stream
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <param name="unfold">Unfold function</param>
        /// <typeparam name="T">Element type-argument</typeparam>
        /// <returns> Flatten source </returns>
        public static IEnumerable<T> Flatten<T>(
            this T source,
            Func<T, IEnumerable<T>> unfold)
        {
            return new[] { source }.Concat(unfold(source).SelectMany(z => Flatten(z, unfold)));
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
            return source.SelectMany(item => item.Flatten(unfold));
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

            IEnumerable<ICollection<T>> seed = sourceColumns
                .Take(1)
                .Single()
                .Select(item => new List<T> { item });

            return sourceColumns
                .Skip(1)
                .Aggregate(seed, Aggregate);

            static IEnumerable<ICollection<T>> Aggregate(IEnumerable<ICollection<T>> acc, IEnumerable<T> next)
            {
                return acc.Join(next,
                    _ => true,
                    _ => true,
                    (left, right) => new List<T>(left) { right });
            }
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
            using (var enumerator = source.GetEnumerator())
            {
                var i = 0;

                while (enumerator.MoveNext())
                {
                    action(enumerator.Current, i++);
                }
            }
        }

        /// <summary> Select collection from untyped IEnumerable </summary>
        /// <param name="enumerable">IEnumerable</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Collection of objects</returns>
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable enumerable)
        {
            return enumerable.GetEnumerator().AsEnumerable<T>();
        }

        /// <summary> Select collection from IEnumerator </summary>
        /// <param name="numerator">IEnumerator</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Collection of objects</returns>
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator numerator)
        {
            while (numerator.MoveNext())
            {
                if (numerator.Current is T typed)
                {
                    yield return typed;
                }
            }
        }

        /// <summary>
        /// Gets IRecursiveEnumerable
        /// </summary>
        /// <param name="source">Source collection</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>IRecursiveEnumerable</returns>
        public static IRecursiveEnumerable<T> MoveNext<T>(this IEnumerable<T> source)
            where T : class
        {
            return new RecursiveEnumerable<T>(source.GetEnumerator());
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
            var items = source.Take(2).ToList();

            if (!items.Any())
            {
                throw new NotFoundException("Source collection is empty");
            }

            if (items.Count != 1)
            {
                throw new AmbiguousMatchException(amb(items));
            }

            return items.Single();
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
            var items = source.Take(2).ToList();

            if (items.Count >= 2)
            {
                throw new AmbiguousMatchException(amb(items));
            }

            return items.SingleOrDefault();
        }
    }
}