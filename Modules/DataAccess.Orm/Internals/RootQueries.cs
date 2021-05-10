namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using GenericDomain.Abstractions;

    internal static class RootQueries
    {
        internal static Expression QueryAll(Type type)
        {
            return Expression.Call(All(type));
        }

        internal static Expression QuerySingle(Type type, Guid key)
        {
            return Expression.Call(Single(type));
        }

        internal static Expression QuerySingleOrDefault(Type type, Guid key)
        {
            return Expression.Call(SingleOrDefault(type));
        }

        private static MethodInfo All(Type type)
        {
            return typeof(ReadRepository<>)
                .MakeGenericType(type)
                .GetRuntimeMethod(nameof(ReadRepository<IEntity>.All), Array.Empty<Type>());
        }

        private static MethodInfo Single(Type type)
        {
            return typeof(ReadRepository<>)
                .MakeGenericType(type)
                .GetRuntimeMethod(nameof(ReadRepository<IEntity>.Single), new[] { typeof(Guid) });
        }

        private static MethodInfo SingleOrDefault(Type type)
        {
            return typeof(ReadRepository<>)
                .MakeGenericType(type)
                .GetRuntimeMethod(nameof(ReadRepository<IEntity>.SingleOrDefault), new[] { typeof(Guid) });
        }
    }
}