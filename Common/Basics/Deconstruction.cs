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
    }
}