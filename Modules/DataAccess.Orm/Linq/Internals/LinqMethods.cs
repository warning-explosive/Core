namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Basics;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;

    internal static class LinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        internal static MethodInfo All(Type itemType)
        {
            return new MethodFinder(typeof(IReadRepository<>).MakeGenericType(itemType),
                    nameof(IReadRepository<IEntity>.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "SpaceEngineers.Core.DataAccess.Contract.Abstractions.IReadRepository<>.All()"));
        }

        internal static MethodInfo QueryableSelect()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Select),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "System.Linq.Queryable.Select()"));
        }

        internal static MethodInfo QueryableWhere()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "System.Linq.Queryable.Where()"));
        }

        internal static MethodInfo QueryableGroupBy2()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.GroupBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(string.Format(CouldNotFindMethodFormat, "System.Linq.Queryable.GroupBy()"));
        }
    }
}