namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Basics;
    using CompositionRoot;

    /// <summary>
    /// AsyncQueryExtensions
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Extracts item type from IQueryable
        /// </summary>
        /// <param name="type">IQueryable</param>
        /// <returns>Item type</returns>
        public static Type ExtractQueryableItemType(this Type type)
        {
            return type
                .ExtractGenericArgumentAtOrSelf(typeof(IQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(ICachedQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(IInsertQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(IUpdateQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(ISetUpdateQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(IFilteredUpdateQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(IDeleteQueryable<>))
                .ExtractGenericArgumentAtOrSelf(typeof(IFilteredDeleteQueryable<>));
        }

        #region ICachedQueryable

        /// <summary>
        /// Adds cache key attribute to query expression
        /// </summary>
        /// <param name="source">Source query</param>
        /// <param name="cacheKey">Cache key</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>ICachedQueryable</returns>
        public static ICachedQueryable<T> CachedExpression<T>(
            this IQueryable<T> source,
            string cacheKey)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.CachedExpression().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                Expression.Constant(cacheKey));

            return (ICachedQueryable<T>)queryable.AsyncQueryProvider.CreateQuery<T>(expression);
        }

        #endregion

        #region IRepository.Update

        /// <summary>
        /// Sets passed values for specified columns
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="set">Set expression</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>ISetUpdateQueryable source</returns>
        public static ISetUpdateQueryable<TEntity> Set<TEntity>(
            this IUpdateQueryable<TEntity> source,
            Expression<Action<TEntity>> set)
        {
            var queryable = (Queryable<TEntity>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.RepositoryUpdateSet().MakeGenericMethod(typeof(TEntity)),
                queryable.Expression,
                set);

            return (ISetUpdateQueryable<TEntity>)queryable
                .AsyncQueryProvider
                .CreateQuery<TEntity>(expression);
        }

        /// <summary>
        /// Sets passed values for specified columns
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="set">Set expression</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>ISetUpdateQueryable source</returns>
        public static ISetUpdateQueryable<TEntity> Set<TEntity>(
            this ISetUpdateQueryable<TEntity> source,
            Expression<Action<TEntity>> set)
        {
            var queryable = (Queryable<TEntity>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.RepositoryChainedUpdateSet().MakeGenericMethod(typeof(TEntity)),
                queryable.Expression,
                set);

            return (ISetUpdateQueryable<TEntity>)queryable
                .AsyncQueryProvider
                .CreateQuery<TEntity>(expression);
        }

        /// <summary>
        /// Filters update query with specified predicate
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate expression</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>ISetUpdateQueryable source</returns>
        public static IFilteredUpdateQueryable<TEntity> Where<TEntity>(
            this ISetUpdateQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> predicate)
        {
            var queryable = (Queryable<TEntity>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.RepositoryUpdateWhere().MakeGenericMethod(typeof(TEntity)),
                queryable.Expression,
                predicate);

            return (IFilteredUpdateQueryable<TEntity>)queryable
                .AsyncQueryProvider
                .CreateQuery<TEntity>(expression);
        }

        /// <summary>
        /// Represents assign binary operator
        /// </summary>
        /// <param name="left">left operand</param>
        /// <param name="right">right operand</param>
        /// <typeparam name="T">T type-argument</typeparam>
        public static void Assign<T>(this T left, T right)
        {
            throw new InvalidOperationException($"Method {nameof(Assign)} shouldn't be used outside of expression trees");
        }

        #endregion

        #region IRepository.Delete

        /// <summary>
        /// Filters delete query with specified predicate
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate expression</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>IDeleteQueryable source</returns>
        public static IFilteredDeleteQueryable<TEntity> Where<TEntity>(
            this IDeleteQueryable<TEntity> source,
            Expression<Func<TEntity, bool>> predicate)
        {
            var queryable = (Queryable<TEntity>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.RepositoryDeleteWhere().MakeGenericMethod(typeof(TEntity)),
                queryable.Expression,
                predicate);

            return (IFilteredDeleteQueryable<TEntity>)queryable
                .AsyncQueryProvider
                .CreateQuery<TEntity>(expression);
        }

        #endregion

        #region IRepository.All

        /// <summary>
        /// Asynchronously materializes query to array
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<T[]> ToArrayAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            return (await ToListAsync(source, token).ConfigureAwait(false)).ToArray();
        }

        /// <summary>
        /// Asynchronously materializes query to list
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<List<T>> ToListAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var list = new List<T>();

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<T>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<HashSet<T>> ToHashSetAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var hashSet = new HashSet<T>();

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<T>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<HashSet<T>> ToHashSetAsync<T>(
            this ICachedQueryable<T> source,
            IEqualityComparer<T> comparer,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var hashSet = new HashSet<T>(comparer);

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<T>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this ICachedQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            CancellationToken token)
        {
            var queryable = (Queryable<TSource>)source;

            var dictionary = new Dictionary<TKey, TSource>();

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<TSource>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
            this ICachedQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token)
        {
            var queryable = (Queryable<TSource>)source;

            var dictionary = new Dictionary<TKey, TSource>(comparer);

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<TSource>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="elementSelector">Element selector</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TElement">TElement type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this ICachedQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            CancellationToken token)
        {
            var queryable = (Queryable<TSource>)source;

            var dictionary = new Dictionary<TKey, TElement>();

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<TSource>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="elementSelector">Element selector</param>
        /// <param name="comparer">Equality comparer</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TSource">TSource type-argument</typeparam>
        /// <typeparam name="TKey">TKey type-argument</typeparam>
        /// <typeparam name="TElement">TElement type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
            this ICachedQueryable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer,
            CancellationToken token)
        {
            var queryable = (Queryable<TSource>)source;

            var dictionary = new Dictionary<TKey, TElement>(comparer);

            var asyncSource = queryable
                .AsyncQueryProvider
                .ExecuteAsync<TSource>(queryable.Expression, token)
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
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T> FirstAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirst().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T> FirstAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirst2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first or default scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> FirstOrDefaultAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirstOrDefault().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to first or default scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> FirstOrDefaultAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableFirstOrDefault2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public static Task<T> SingleAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingle().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        [SuppressMessage("Analysis", "CA1720", Justification = "desired name")]
        public static Task<T> SingleAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingle2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single or default scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> SingleOrDefaultAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingleOrDefault().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to single or default scalar value
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<T?> SingleOrDefaultAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableSingleOrDefault2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<T?>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AnyAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAny().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AnyAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAny2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<bool> AllAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableAll().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<bool>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<int> CountAsync<T>(
            this ICachedQueryable<T> source,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableCount().MakeGenericMethod(typeof(T)),
                queryable.Expression);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<int>(expression, token);
        }

        /// <summary>
        /// Asynchronously materializes query to check result
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing operation</returns>
        public static Task<int> CountAsync<T>(
            this ICachedQueryable<T> source,
            Expression<Func<T, bool>> predicate,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.QueryableCount2().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                predicate);

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<int>(expression, token);
        }

        #endregion

        internal static IInsertQueryable<IDatabaseEntity> WithDependencyContainer(
            this IInsertQueryable<IDatabaseEntity> source,
            IDependencyContainer dependencyContainer)
        {
            throw new InvalidOperationException($"Method {nameof(WithDependencyContainer)} shouldn't be used outside of expression trees");
        }
    }
}