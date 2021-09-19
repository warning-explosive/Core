namespace SpaceEngineers.Core.DataAccess.Api.Reading
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// AsyncQueryExtensions
    /// </summary>
    public static class AsyncQueryExtensions
    {
        /// <summary>
        /// Asynchronously materializes query to array
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [Pure]
        public static Task<T[]> ToArrayAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            return Task.FromResult(query.ToArray());
        }

        /// <summary>
        /// Asynchronously materializes query to list
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [Pure]
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            return Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Asynchronously materializes query to dictionary
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            var dictionary = query.ToDictionary(keySelector);
            return Task.FromResult(dictionary);
        }

        /// <summary>
        /// Asynchronously materializes query to dictionary
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="elementSelector">Element selector</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TElement">TElement type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            var dictionary = query.ToDictionary(keySelector, elementSelector);
            return Task.FromResult(dictionary);
        }

        /// <summary>
        /// Asynchronously materializes query to first scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [Pure]
        public static Task<T> FirstAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            return Task.FromResult(query.First());
        }

        /// <summary>
        /// Asynchronously materializes query to first or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [Pure]
        public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            var value = query.FirstOrDefault();
            return Task.FromResult((T?)value);
        }

        /// <summary>
        /// Asynchronously materializes query to single scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        [Pure]
        public static Task<T> SingleAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            return Task.FromResult(query.Single());
        }

        /// <summary>
        /// Asynchronously materializes query to single or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [Pure]
        public static Task<T?> SingleOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            // TODO: #134 - IAsyncQueryable extensions
            _ = token;
            var value = query.SingleOrDefault();
            return Task.FromResult((T?)value);
        }
    }
}