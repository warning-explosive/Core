namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
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
        public static async Task<T[]> ToArrayAsync<T>(
            this IQueryable<T> query,
            CancellationToken token)
        {
            return (await ToListAsync(query, token).ConfigureAwait(false)).ToArray();
        }

        /// <summary>
        /// Asynchronously materializes query to list
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<List<T>> ToListAsync<T>(
            this IQueryable<T> query,
            CancellationToken token)
        {
            var list = new List<T>();

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<T>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Asynchronously materializes query to hashset
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<HashSet<T>> ToHashSetAsync<T>(
            this IQueryable<T> query,
            CancellationToken token)
        {
            var hashSet = new HashSet<T>();

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<T>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                hashSet.Add(item);
            }

            return hashSet;
        }

        /// <summary>
        /// Asynchronously materializes query to hashset
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<HashSet<T>> ToHashSetAsync<T>(
            this IQueryable<T> query,
            IEqualityComparer<T> comparer,
            CancellationToken token)
        {
            var hashSet = new HashSet<T>(comparer);

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<T>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                hashSet.Add(item);
            }

            return hashSet;
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
        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            CancellationToken token)
        {
            var dictionary = new Dictionary<TKey, TSource>();

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<TSource>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                dictionary.Add(keySelector(item), item);
            }

            return dictionary;
        }

        /// <summary>
        /// Asynchronously materializes query to dictionary
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token)
        {
            var dictionary = new Dictionary<TKey, TSource>(comparer);

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<TSource>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                dictionary.Add(keySelector(item), item);
            }

            return dictionary;
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
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken token)
        {
            var dictionary = new Dictionary<TKey, TElement>();

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<TSource>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                dictionary.Add(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }

        /// <summary>
        /// Asynchronously materializes query to dictionary
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="elementSelector">Element selector</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TElement">TElement type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this IQueryable<TSource> query,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token)
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            var asyncSource = ((IAsyncQueryProvider)query.Provider)
                .ExecuteAsync<TSource>(query.Expression, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in asyncSource)
            {
                dictionary.Add(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }

        /// <summary>
        /// Asynchronously materializes query to first scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T> FirstAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirst().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T> FirstAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirst2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirstOrDefault().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> FirstOrDefaultAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirstOrDefault2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public static Task<T> SingleAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingle().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public static Task<T> SingleAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingle2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> SingleOrDefaultAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingleOrDefault().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single or default scalar value
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> SingleOrDefaultAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingleOrDefault2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AnyAsync<T>(this IQueryable<T> query, CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAny().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AnyAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAny2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AllAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAll().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<int> CountAsync<T>(
            this IQueryable<T> query,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableCount().MakeGenericMethod(typeof(T)),
                query.Expression);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<int>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<int> CountAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableCount2().MakeGenericMethod(typeof(T)),
                query.Expression,
                predicate);

            return ((IAsyncQueryProvider)query.Provider).ExecuteScalarAsync<int>(expression, token);
        }
    }
}