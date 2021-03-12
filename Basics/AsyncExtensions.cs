namespace SpaceEngineers.Core.Basics
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncExtensions
    /// </summary>
    public static class AsyncExtensions
    {
        /// <summary>
        /// WhenAll extension method
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Composite WhenAll task</returns>
        public static Task WhenAll(this IEnumerable<Task> source)
        {
            return Task.WhenAll(source);
        }

        /// <summary>
        /// WhenAll extension method
        /// </summary>
        /// <param name="source">Source</param>
        /// <typeparam name="TResult">TResult type-argument</typeparam>
        /// <returns>Composite WhenAll task</returns>
        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> source)
        {
            return Task.WhenAll(source);
        }
    }
}