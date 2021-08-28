namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Contract.Abstractions;
    using GenericDomain.Api.Abstractions;

    internal static class RootQueries
    {
        internal static Expression QueryAll<T>(this IReadRepository<T> readRepository)
            where T : class, IEntity
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                All<T>());
        }

        internal static Expression QuerySingle<T>(this IReadRepository<T> readRepository, Guid key)
            where T : class, IEntity
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                Single<T>(),
                Expression.Constant(key));
        }

        internal static Expression QuerySingleOrDefault<T>(this IReadRepository<T> readRepository, Guid key)
            where T : class, IEntity
        {
            return Expression.Call(
                Expression.Constant(readRepository),
                SingleOrDefault<T>(),
                Expression.Constant(key));
        }

        private static MethodInfo All<T>()
            where T : class, IEntity
        {
            return typeof(IReadRepository<T>)
                .GetRuntimeMethod(nameof(IReadRepository<IEntity>.All), Array.Empty<Type>());
        }

        private static MethodInfo Single<T>()
            where T : class, IEntity
        {
            return typeof(IReadRepository<T>)
                .GetRuntimeMethod(nameof(IReadRepository<IEntity>.Single), new[] { typeof(Guid) });
        }

        private static MethodInfo SingleOrDefault<T>()
            where T : class, IEntity
        {
            return typeof(IReadRepository<T>)
                .GetRuntimeMethod(nameof(IReadRepository<IEntity>.SingleOrDefault), new[] { typeof(Guid) });
        }
    }
}