namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Collections.Generic;

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
    }
}