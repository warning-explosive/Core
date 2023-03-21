namespace SpaceEngineers.Core.Basics
{
    using System.Collections.Generic;
    using System.Threading;
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

        /// <summary>
        /// Converts Enumerable source to IAsyncEnumerable source
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Enumerable over async enumerable source</returns>
        public static async Task<IEnumerable<T>> AsEnumerable<T>(this IAsyncEnumerable<T> source, CancellationToken token)
        {
            var list = new List<T>();

            var asyncSource = source
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                list.Add(item);
            }

            return list;
        }
    }
}