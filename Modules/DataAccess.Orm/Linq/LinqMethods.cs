namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Reading;
    using Basics;

    /// <summary>
    /// LinqMethods
    /// </summary>
    public static class LinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        private static MethodInfo? _repositoryAll;
        private static MethodInfo? _cachedExpression;
        private static MethodInfo? _queryableSingle;
        private static MethodInfo? _queryableSingle2;
        private static MethodInfo? _queryableSingleOrDefault;
        private static MethodInfo? _queryableSingleOrDefault2;
        private static MethodInfo? _queryableFirst;
        private static MethodInfo? _queryableFirst2;
        private static MethodInfo? _queryableFirstOrDefault;
        private static MethodInfo? _queryableFirstOrDefault2;
        private static MethodInfo? _queryableSelect;
        private static MethodInfo? _queryableWhere;
        private static MethodInfo? _queryableAny;
        private static MethodInfo? _queryableAny2;
        private static MethodInfo? _queryableAll;
        private static MethodInfo? _queryableCount;
        private static MethodInfo? _queryableCount2;
        private static MethodInfo? _queryableGroupBy2;
        private static MethodInfo? _queryableGroupBy3;
        private static MethodInfo? _queryableContains;
        private static MethodInfo? _queryableDistinct;
        private static MethodInfo? _queryableOrderBy;
        private static MethodInfo? _queryableOrderByDescending;
        private static MethodInfo? _queryableThenBy;
        private static MethodInfo? _queryableThenByDescending;
        private static MethodInfo? _queryableSelectMany;

        /// <summary>
        /// Does method represent query root (IReadRepository.All())
        /// </summary>
        /// <param name="method">MethodInfo</param>
        /// <returns>IsQueryRoot</returns>
        public static bool IsQueryRoot(this MethodInfo method)
        {
            return method.Name.Equals(nameof(IReadRepository.All), StringComparison.Ordinal)
                   && method.ReflectedType == typeof(IReadRepository);
        }

        /// <summary>
        /// IReadRepository.All
        /// </summary>
        /// <returns>IReadRepository.All MethodInfo</returns>
        public static MethodInfo RepositoryAll()
        {
            return _repositoryAll ??= new MethodFinder(typeof(IReadRepository),
                    nameof(IReadRepository.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Api.Abstractions.IReadRepository.All()"));
        }

        /// <summary>
        /// AsyncQueryExtensions.CachedExpression
        /// </summary>
        /// <returns>AsyncQueryExtensions.CachedExpression MethodInfo</returns>
        public static MethodInfo CachedExpression()
        {
            return _cachedExpression ??= new MethodFinder(typeof(AsyncQueryExtensions),
                    nameof(AsyncQueryExtensions.CachedExpression),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(string) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Api.Abstractions.IReadRepository.All()"));
        }

        /// <summary>
        /// Queryable.Single
        /// </summary>
        /// <returns>Queryable.Single MethodInfo</returns>
        public static MethodInfo QueryableSingle()
        {
            return _queryableSingle ??= new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Single),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Single()"));
        }

        /// <summary>
        /// Queryable.Single
        /// </summary>
        /// <returns>Queryable.Single MethodInfo</returns>
        public static MethodInfo QueryableSingle2()
        {
            return _queryableSingle2 ??= new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Single),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Single()"));
        }

        /// <summary>
        /// Queryable.SingleOrDefault
        /// </summary>
        /// <returns>Queryable.SingleOrDefault MethodInfo</returns>
        public static MethodInfo QueryableSingleOrDefault()
        {
            return _queryableSingleOrDefault ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.SingleOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.SingleOrDefault()"));
        }

        /// <summary>
        /// Queryable.SingleOrDefault
        /// </summary>
        /// <returns>Queryable.SingleOrDefault MethodInfo</returns>
        public static MethodInfo QueryableSingleOrDefault2()
        {
            return _queryableSingleOrDefault2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.SingleOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.SingleOrDefault()"));
        }

        /// <summary>
        /// Queryable.First
        /// </summary>
        /// <returns>Queryable.First MethodInfo</returns>
        public static MethodInfo QueryableFirst()
        {
            return _queryableFirst ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.First),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.First()"));
        }

        /// <summary>
        /// Queryable.First
        /// </summary>
        /// <returns>Queryable.First MethodInfo</returns>
        public static MethodInfo QueryableFirst2()
        {
            return _queryableFirst2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.First),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.First()"));
        }

        /// <summary>
        /// Queryable.FirstOrDefault
        /// </summary>
        /// <returns>Queryable.FirstOrDefault MethodInfo</returns>
        public static MethodInfo QueryableFirstOrDefault()
        {
            return _queryableFirstOrDefault ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.FirstOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.FirstOrDefault()"));
        }

        /// <summary>
        /// Queryable.FirstOrDefault
        /// </summary>
        /// <returns>Queryable.FirstOrDefault MethodInfo</returns>
        public static MethodInfo QueryableFirstOrDefault2()
        {
            return _queryableFirstOrDefault2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.FirstOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.FirstOrDefault()"));
        }

        /// <summary>
        /// Queryable.Select
        /// </summary>
        /// <returns>Queryable.Select MethodInfo</returns>
        public static MethodInfo QueryableSelect()
        {
            return _queryableSelect ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Select),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Select()"));
        }

        /// <summary>
        /// Queryable.Where
        /// </summary>
        /// <returns>Queryable.Where MethodInfo</returns>
        public static MethodInfo QueryableWhere()
        {
            return _queryableWhere ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Where()"));
        }

        /// <summary>
        /// Queryable.Any
        /// </summary>
        /// <returns>Queryable.Any MethodInfo</returns>
        public static MethodInfo QueryableAny()
        {
            return _queryableAny ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Any),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Any()"));
        }

        /// <summary>
        /// Queryable.Any
        /// </summary>
        /// <returns>Queryable.Any MethodInfo</returns>
        public static MethodInfo QueryableAny2()
        {
            return _queryableAny2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Any),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Any()"));
        }

        /// <summary>
        /// Queryable.All
        /// </summary>
        /// <returns>Queryable.All MethodInfo</returns>
        public static MethodInfo QueryableAll()
        {
            return _queryableAll ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.All),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        /// <summary>
        /// Queryable.Count
        /// </summary>
        /// <returns>Queryable.Count MethodInfo</returns>
        public static MethodInfo QueryableCount()
        {
            return _queryableCount ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Count),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        /// <summary>
        /// Queryable.Count
        /// </summary>
        /// <returns>Queryable.Count MethodInfo</returns>
        public static MethodInfo QueryableCount2()
        {
            return _queryableCount2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Count),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        /// <summary>
        /// Queryable.GroupBy
        /// </summary>
        /// <returns>Queryable.GroupBy MethodInfo</returns>
        public static MethodInfo QueryableGroupBy2()
        {
            return _queryableGroupBy2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.GroupBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.GroupBy()"));
        }

        /// <summary>
        /// Queryable.GroupBy
        /// </summary>
        /// <returns>Queryable.GroupBy MethodInfo</returns>
        public static MethodInfo QueryableGroupBy3()
        {
            return _queryableGroupBy3 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.GroupBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.GroupBy()"));
        }

        /// <summary>
        /// Queryable.Contains
        /// </summary>
        /// <returns>Queryable.Contains MethodInfo</returns>
        public static MethodInfo QueryableContains()
        {
            return _queryableContains ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Contains),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(object) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Contains()"));
        }

        /// <summary>
        /// Queryable.Distinct
        /// </summary>
        /// <returns>Queryable.Distinct MethodInfo</returns>
        public static MethodInfo QueryableDistinct()
        {
            return _queryableDistinct ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Distinct),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Distinct()"));
        }

        /// <summary>
        /// Queryable.OrderBy
        /// </summary>
        /// <returns>Queryable.OrderBy MethodInfo</returns>
        public static MethodInfo QueryableOrderBy()
        {
            return _queryableOrderBy ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.OrderBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.OrderBy()"));
        }

        /// <summary>
        /// Queryable.OrderByDescending
        /// </summary>
        /// <returns>Queryable.OrderByDescending MethodInfo</returns>
        public static MethodInfo QueryableOrderByDescending()
        {
            return _queryableOrderByDescending ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.OrderByDescending),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.OrderByDescending()"));
        }

        /// <summary>
        /// Queryable.ThenBy
        /// </summary>
        /// <returns>Queryable.ThenBy MethodInfo</returns>
        public static MethodInfo QueryableThenBy()
        {
            return _queryableThenBy ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.ThenBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IOrderedQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.ThenBy()"));
        }

        /// <summary>
        /// Queryable.ThenByDescending
        /// </summary>
        /// <returns>Queryable.ThenByDescending MethodInfo</returns>
        public static MethodInfo QueryableThenByDescending()
        {
            return _queryableThenByDescending ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.ThenByDescending),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IOrderedQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.ThenByDescending()"));
        }

        /// <summary>
        /// Queryable.SelectMany
        /// </summary>
        /// <returns>Queryable.SelectMany MethodInfo</returns>
        public static MethodInfo QueryableSelectMany()
        {
            return _queryableSelectMany ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.SelectMany),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, IEnumerable<object>>>) }
                }
               .FindMethod()
               .EnsureNotNull(CouldNotFindMethodFormat.Format("System.Linq.Queryable.SelectMany()"));
        }
    }
}