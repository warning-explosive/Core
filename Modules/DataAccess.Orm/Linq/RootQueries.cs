namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Abstractions;

    internal static class RootQueries
    {
        internal static Expression QueryAll<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository)
            where TEntity : IUniqueIdentified<TKey>
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                All<TEntity, TKey>());
        }

        internal static Expression QuerySingle<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository,
            TKey key)
            where TEntity : IUniqueIdentified<TKey>
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                Single<TEntity, TKey>(),
                Expression.Constant(key));
        }

        internal static Expression QuerySingleOrDefault<TEntity, TKey>(
            this IReadRepository<TEntity, TKey> readRepository,
            TKey key)
            where TEntity : IUniqueIdentified<TKey>
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                SingleOrDefault<TEntity, TKey>(),
                Expression.Constant(key));
        }

        private static MethodInfo All<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.All), Array.Empty<Type>());
        }

        private static MethodInfo Single<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.SingleAsync), new[] { typeof(TKey) });
        }

        private static MethodInfo SingleOrDefault<TEntity, TKey>()
            where TEntity : IUniqueIdentified<TKey>
        {
            return typeof(IReadRepository<TEntity, TKey>)
                .GetRuntimeMethod(nameof(IReadRepository<TEntity, TKey>.SingleOrDefaultAsync), new[] { typeof(TKey) });
        }
    }
}