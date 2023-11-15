namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CompositionRoot;
    using Model;
    using Transaction;

    /// <summary>
    /// AsyncQueryExtensions
    /// </summary>
    public static class LinqExtensions
    {
        #region IRepository.Insert

        /// <summary>
        /// Adds cache key attribute to query expression
        /// </summary>
        /// <param name="source">Source query</param>
        /// <param name="cacheKey">Cache key</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>ICachedInsertQueryable</returns>
        public static ICachedInsertQueryable<T> CachedExpression<T>(
            this IInsertQueryable<T> source,
            string cacheKey)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.CachedInsertExpression().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                Expression.Constant(cacheKey));

            return (ICachedInsertQueryable<T>)queryable
                .AsyncQueryProvider
                .CreateQuery<T>(expression);
        }

        /// <summary>
        /// Executes insert query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedInsertQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (dependencyContainer, transaction, entities, insertBehavior, cacheKey) = InsertCommandExpressionVisitor.Extract(queryable.Expression);

            var modelProvider = dependencyContainer.Resolve<IModelProvider>();

            var version = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            foreach (var entity in entities.SelectMany(modelProvider.Flatten).OfType<IDatabaseEntity>())
            {
                entity.Version = version;
            }

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            transaction.CollectChange(new CreateEntityChange(entities, insertBehavior, cacheKey));

            return affectedRowsCount;
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
        /// Adds cache key attribute to query expression
        /// </summary>
        /// <param name="source">Source query</param>
        /// <param name="cacheKey">Cache key</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>ICachedUpdateQueryable</returns>
        public static ICachedUpdateQueryable<T> CachedExpression<T>(
            this IFilteredUpdateQueryable<T> source,
            string cacheKey)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.CachedUpdateExpression().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                Expression.Constant(cacheKey));

            return (ICachedUpdateQueryable<T>)queryable
                .AsyncQueryProvider
                .CreateQuery<T>(expression);
        }

        /// <summary>
        /// Executes update query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedUpdateQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, setExpressions, predicate, cacheKey) = UpdateCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = await GetVersions(transaction, predicate, cacheKey, token).ConfigureAwait(false);

            var updateVersion = await transaction
                .GetVersion(token)
                .ConfigureAwait(false);

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new UpdateEntityChange<TEntity>(version, updateVersion, count, setExpressions, predicate, cacheKey));
            }

            return affectedRowsCount;
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

        /// <summary>
        /// Adds cache key attribute to query expression
        /// </summary>
        /// <param name="source">Source query</param>
        /// <param name="cacheKey">Cache key</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>ICachedDeleteQueryable</returns>
        public static ICachedDeleteQueryable<T> CachedExpression<T>(
            this IFilteredDeleteQueryable<T> source,
            string cacheKey)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.CachedDeleteExpression().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                Expression.Constant(cacheKey));

            return (ICachedDeleteQueryable<T>)queryable
                .AsyncQueryProvider
                .CreateQuery<T>(expression);
        }

        /// <summary>
        /// Executes delete query
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Affected rows count</returns>
        public static async Task<long> Invoke<TEntity>(
            this ICachedDeleteQueryable<TEntity> source,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var queryable = (Queryable<TEntity>)source;

            var (transaction, predicate, cacheKey) = DeleteCommandExpressionVisitor<TEntity>.Extract(queryable.Expression);

            var versions = await GetVersions(transaction, predicate, cacheKey, token).ConfigureAwait(false);

            var affectedRowsCount = await queryable
                .AsyncQueryProvider
                .ExecuteNonQueryAsync(queryable.Expression, token)
                .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                transaction.CollectChange(new DeleteEntityChange<TEntity>(version, count, predicate, cacheKey));
            }

            return affectedRowsCount;
        }

        #endregion

        #region IRepository.All

        /// <summary>
        /// Gets query plan from database
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="analyze">Invokes sql command and grab statistics according to default_statistics_target</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>ICachedQueryable</returns>
        public static Task<string> Explain<T>(
            this ICachedQueryable<T> source,
            bool analyze,
            CancellationToken token)
        {
            var queryable = (Queryable<T>)source;

            var expression = Expression.Call(
                null,
                LinqMethods.Explain().MakeGenericMethod(typeof(T)),
                queryable.Expression,
                Expression.Constant(analyze),
                Expression.Constant(token));

            return queryable
                .AsyncQueryProvider
                .ExecuteScalarAsync<string>(expression, token);
        }

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

            return (ICachedQueryable<T>)queryable
                .AsyncQueryProvider
                .CreateQuery<T>(expression);
        }

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

        internal static Type ExtractQueryableItemType(this Type type)
        {
            return (typeof(IQueryable).IsAssignableFrom(type) || typeof(ICustomQueryable).IsAssignableFrom(type))
                   && type.IsGenericType
                ? type.GetGenericArguments()[0]
                : type;
        }

        internal static IInsertQueryable<IDatabaseEntity> WithDependencyContainer(
            this IInsertQueryable<IDatabaseEntity> source,
            IDependencyContainer dependencyContainer)
        {
            throw new InvalidOperationException($"Method {nameof(WithDependencyContainer)} shouldn't be used outside of expression trees");
        }

        private static async Task<Dictionary<long, int>> GetVersions<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, bool>> predicate,
            string cacheKey,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return (await transaction
                    .All<TEntity>()
                    .Where(predicate)
                    .Select(entity => entity.Version)
                    .CachedExpression($"{nameof(GetVersions)}:{cacheKey}")
                    .ToListAsync(token)
                    .ConfigureAwait(false))
                .GroupBy(version => version)
                .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());
        }
    }
}