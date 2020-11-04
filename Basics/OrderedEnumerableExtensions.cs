namespace SpaceEngineers.Core.Basics
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IOrderedEnumerable extensions
    /// </summary>
    public static class OrderedEnumerableExtensions
    {
        /// <summary>
        /// Converts IEnumerable to IOrderedEnumerable with order keeping
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <typeparam name="T">Item type-argument</typeparam>
        /// <returns>Ordered stream</returns>
        public static IOrderedEnumerable<T> AsOrderedEnumerable<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(_ => 1);
        }
    }
}