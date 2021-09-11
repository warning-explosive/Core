namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Abstractions;
    using Basics;

    internal static class LinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        internal static MethodInfo All(Type itemType, Type primaryKeyType)
        {
            return new MethodFinder(typeof(IReadRepository<,>).MakeGenericType(itemType, primaryKeyType),
                    nameof(IReadRepository<IDatabaseEntity<Guid>, Guid>.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Api.Abstractions.IReadRepository<>.All()"));
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
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Select()"));
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
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Where()"));
        }

        internal static MethodInfo QueryableGroupBy2()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.GroupBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.GroupBy()"));
        }

        internal static MethodInfo QueryableGroupBy3()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.GroupBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.GroupBy()"));
        }
    }
}