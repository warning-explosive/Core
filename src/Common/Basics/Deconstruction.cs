namespace SpaceEngineers.Core.Basics
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Deconstruction
    /// </summary>
    public static class Deconstruction
    {
        /// <summary>
        /// Deconstructs IGrouping
        /// </summary>
        /// <param name="grouping">IGrouping</param>
        /// <param name="key">Key</param>
        /// <param name="values">Values</param>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TValue">TValue type-argument</typeparam>
        public static void Deconstruct<TKey, TValue>(
            this IGrouping<TKey, TValue> grouping,
            out TKey key,
            out IEnumerable<TValue> values)
        {
            key = grouping.Key;
            values = grouping;
        }

        /// <summary>
        /// Deconstructs source collection
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="first">First element</param>
        /// <param name="rest">Rest elements</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out IEnumerable<T> rest)
        {
            first = source.FirstOrDefault();
            rest = source.Skip(1);
        }

        /// <summary>
        /// Deconstructs source collection
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="first">First element</param>
        /// <param name="second">Second element</param>
        /// <param name="rest">Rest elements</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out IEnumerable<T> rest)
            => (first, (second, rest)) = source;

        /// <summary>
        /// Deconstructs source collection
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="first">First element</param>
        /// <param name="second">Second element</param>
        /// <param name="third">Third element</param>
        /// <param name="rest">Rest elements</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Deconstruct<T>(this IEnumerable<T> source, out T first, out T second, out T third, out IEnumerable<T> rest)
            => (first, second, (third, rest)) = source;

        /// <summary>
        /// Constructs source stream
        /// </summary>
        /// <param name="source">Source</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Stream</returns>
        public static IEnumerable<T> ConstructEnumerable<T>(this (T first, T second) source)
        {
            var (first, second) = source;

            yield return first;
            yield return second;
        }

        /// <summary>
        /// Constructs source stream
        /// </summary>
        /// <param name="source">Source</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Stream</returns>
        public static IEnumerable<T> ConstructEnumerable<T>(this (T first, T second, T third) source)
        {
            var (first, second, third) = source;

            yield return first;
            yield return second;
            yield return third;
        }
    }
}