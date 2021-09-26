namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Api.Reading;

    internal static class RootQueries
    {
        internal static Expression QueryAll<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                All<TEntity, TKey>());
        }

        internal static Expression QuerySingleAsync<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository,
            TKey key)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                SingleAsync<TEntity, TKey>(),
                Expression.Constant(key));
        }

        internal static Expression QuerySingleOrDefaultAsync<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository,
            TKey key)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                SingleOrDefaultAsync<TEntity, TKey>(),
                Expression.Constant(key));
        }

        private static MethodInfo All<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.All), Array.Empty<Type>());
        }

        private static MethodInfo SingleAsync<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.SingleAsync), new[] { typeof(TKey) });
        }

        private static MethodInfo SingleOrDefaultAsync<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.SingleOrDefaultAsync), new[] { typeof(TKey) });
        }
    }
}