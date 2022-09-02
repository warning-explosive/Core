namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Model;
    using Api.Reading;
    using Basics;

    /// <summary>
    /// LinqMethods
    /// </summary>
    public static class LinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        /// <summary>
        /// IReadRepository.All
        /// </summary>
        /// <param name="itemType">Item type</param>
        /// <returns>IReadRepository.All MethodInfo</returns>
        public static MethodInfo All(Type itemType)
        {
            return new MethodFinder(typeof(IReadRepository<>).MakeGenericType(itemType),
                    nameof(IReadRepository<IDatabaseEntity<Guid>>.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .FindMethod()
                .EnsureNotNull(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Api.Abstractions.IReadRepository<>.All()"));
        }

        /// <summary>
        /// Queryable.Single
        /// </summary>
        /// <returns>Queryable.Single MethodInfo</returns>
        public static MethodInfo QueryableSingle()
        {
            return new MethodFinder(typeof(Queryable),
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
            return new MethodFinder(typeof(Queryable),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.SingleOrDefault),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.SingleOrDefault),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.First),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.First),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.FirstOrDefault),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.FirstOrDefault),
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

        /// <summary>
        /// Queryable.Where
        /// </summary>
        /// <returns>Queryable.Where MethodInfo</returns>
        public static MethodInfo QueryableWhere()
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

        /// <summary>
        /// Queryable.Any
        /// </summary>
        /// <returns>Queryable.Any MethodInfo</returns>
        public static MethodInfo QueryableAny()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Any),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Any),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.All),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Count),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Count),
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

        /// <summary>
        /// Queryable.GroupBy
        /// </summary>
        /// <returns>Queryable.GroupBy MethodInfo</returns>
        public static MethodInfo QueryableGroupBy3()
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

        /// <summary>
        /// Queryable.Contains
        /// </summary>
        /// <returns>Queryable.Contains MethodInfo</returns>
        public static MethodInfo QueryableContains()
        {
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Contains),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Distinct),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.OrderBy),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.OrderByDescending),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.ThenBy),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.ThenByDescending),
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
            return new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.SelectMany),
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