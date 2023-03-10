namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using Basics;
    using CompositionRoot;
    using Model;
    using Transaction;

    internal static class LinqMethods
    {
        private const string CouldNotFindMethodFormat = "Could not find {0} method";

        private static MethodInfo? _explain;
        private static MethodInfo? _repositoryAll;
        private static MethodInfo? _cachedExpression;
        private static MethodInfo? _repositoryInsert;
        private static MethodInfo? _cachedInsertExpression;
        private static MethodInfo? _withDependencyContainer;
        private static MethodInfo? _repositoryUpdate;
        private static MethodInfo? _repositoryUpdateWhere;
        private static MethodInfo? _repositoryUpdateSet;
        private static MethodInfo? _repositoryChainedUpdateSet;
        private static MethodInfo? _cachedUpdateExpression;
        private static MethodInfo? _repositoryDelete;
        private static MethodInfo? _repositoryDeleteWhere;
        private static MethodInfo? _cachedDeleteExpression;
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
        private static MethodInfo? _queryableContains;
        private static MethodInfo? _queryableDistinct;
        private static MethodInfo? _queryableOrderBy;
        private static MethodInfo? _queryableOrderByDescending;
        private static MethodInfo? _queryableThenBy;
        private static MethodInfo? _queryableThenByDescending;
        private static MethodInfo? _queryableCast;
        private static MethodInfo? _like;
        private static MethodInfo? _isNull;
        private static MethodInfo? _isNotNull;
        private static MethodInfo? _assign;

        public static MethodInfo Explain()
        {
            return _explain ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.Explain),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(ICachedQueryable<object>), typeof(bool), typeof(CancellationToken) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Sql.Linq.LinqExtensions.Explain()"));
        }

        public static MethodInfo RepositoryAll()
        {
            return _repositoryAll ??= new MethodFinder(typeof(IRepository),
                    nameof(IRepository.All),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(IUniqueIdentified) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.IRepository.All()"));
        }

        public static MethodInfo CachedExpression()
        {
            return _cachedExpression ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.CachedExpression),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.CachedExpression()"));
        }

        public static MethodInfo RepositoryInsert()
        {
            return _repositoryInsert ??= new MethodFinder(typeof(IRepository),
                    nameof(IRepository.Insert),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(IDatabaseContext), typeof(IReadOnlyCollection<IDatabaseEntity>), typeof(EnInsertBehavior) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.IRepository.Insert()"));
        }

        public static MethodInfo CachedInsertExpression()
        {
            return _cachedInsertExpression ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.CachedExpression),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IInsertQueryable<object>), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.CachedExpression()"));
        }

        public static MethodInfo WithDependencyContainer()
        {
            return _withDependencyContainer ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.WithDependencyContainer),
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(IInsertQueryable<IDatabaseEntity>), typeof(IDependencyContainer) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.WithDependencyContainer()"));
        }

        public static MethodInfo RepositoryUpdate()
        {
            return _repositoryUpdate ??= new MethodFinder(typeof(IRepository),
                    nameof(IRepository.Update),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(IDatabaseEntity) },
                    ArgumentTypes = new[] { typeof(IDatabaseContext) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.IRepository.Update()"));
        }

        public static MethodInfo RepositoryUpdateWhere()
        {
            return _repositoryUpdateWhere ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(ISetUpdateQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.Where()"));
        }

        public static MethodInfo RepositoryUpdateSet()
        {
            return _repositoryUpdateSet ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.Set),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IUpdateQueryable<object>), typeof(Expression<Action<object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.Set()"));
        }

        public static MethodInfo RepositoryChainedUpdateSet()
        {
            return _repositoryChainedUpdateSet ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.Set),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(ISetUpdateQueryable<object>), typeof(Expression<Action<object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.Set()"));
        }

        public static MethodInfo CachedUpdateExpression()
        {
            return _cachedUpdateExpression ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.CachedExpression),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IFilteredUpdateQueryable<object>), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.CachedExpression()"));
        }

        public static MethodInfo RepositoryDelete()
        {
            return _repositoryDelete ??= new MethodFinder(typeof(IRepository),
                    nameof(IRepository.Delete),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(IDatabaseEntity) },
                    ArgumentTypes = new[] { typeof(IDatabaseContext) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.IRepository.Delete()"));
        }

        public static MethodInfo RepositoryDeleteWhere()
        {
            return _repositoryDeleteWhere ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IDeleteQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.Where()"));
        }

        public static MethodInfo CachedDeleteExpression()
        {
            return _cachedDeleteExpression ??= new MethodFinder(typeof(LinqExtensions),
                    nameof(LinqExtensions.CachedExpression),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IFilteredDeleteQueryable<object>), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.LinqExtensions.CachedExpression()"));
        }

        public static MethodInfo QueryableSingle()
        {
            return _queryableSingle ??= new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Single),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Single()"));
        }

        public static MethodInfo QueryableSingle2()
        {
            return _queryableSingle2 ??= new MethodFinder(typeof(Queryable),
                    nameof(System.Linq.Queryable.Single),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Single()"));
        }

        public static MethodInfo QueryableSingleOrDefault()
        {
            return _queryableSingleOrDefault ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.SingleOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.SingleOrDefault()"));
        }

        public static MethodInfo QueryableSingleOrDefault2()
        {
            return _queryableSingleOrDefault2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.SingleOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.SingleOrDefault()"));
        }

        public static MethodInfo QueryableFirst()
        {
            return _queryableFirst ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.First),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.First()"));
        }

        public static MethodInfo QueryableFirst2()
        {
            return _queryableFirst2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.First),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.First()"));
        }

        public static MethodInfo QueryableFirstOrDefault()
        {
            return _queryableFirstOrDefault ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.FirstOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.FirstOrDefault()"));
        }

        public static MethodInfo QueryableFirstOrDefault2()
        {
            return _queryableFirstOrDefault2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.FirstOrDefault),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.FirstOrDefault()"));
        }

        public static MethodInfo QueryableSelect()
        {
            return _queryableSelect ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Select),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Select()"));
        }

        public static MethodInfo QueryableWhere()
        {
            return _queryableWhere ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Where),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Where()"));
        }

        public static MethodInfo QueryableAny()
        {
            return _queryableAny ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Any),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Any()"));
        }

        public static MethodInfo QueryableAny2()
        {
            return _queryableAny2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Any),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Any()"));
        }

        public static MethodInfo QueryableAll()
        {
            return _queryableAll ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.All),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        public static MethodInfo QueryableCount()
        {
            return _queryableCount ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Count),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        public static MethodInfo QueryableCount2()
        {
            return _queryableCount2 ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Count),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(string) },
                    ArgumentTypes = new[] { typeof(IQueryable<string>), typeof(Expression<Func<string, bool>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.All()"));
        }

        public static MethodInfo QueryableContains()
        {
            return _queryableContains ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Contains),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Contains()"));
        }

        public static MethodInfo QueryableDistinct()
        {
            return _queryableDistinct ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Distinct),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Distinct()"));
        }

        public static MethodInfo QueryableOrderBy()
        {
            return _queryableOrderBy ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.OrderBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.OrderBy()"));
        }

        public static MethodInfo QueryableOrderByDescending()
        {
            return _queryableOrderByDescending ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.OrderByDescending),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.OrderByDescending()"));
        }

        public static MethodInfo QueryableThenBy()
        {
            return _queryableThenBy ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.ThenBy),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IOrderedQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.ThenBy()"));
        }

        public static MethodInfo QueryableThenByDescending()
        {
            return _queryableThenByDescending ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.ThenByDescending),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object), typeof(object) },
                    ArgumentTypes = new[] { typeof(IOrderedQueryable<object>), typeof(Expression<Func<object, object>>) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.ThenByDescending()"));
        }

        public static MethodInfo QueryableCast()
        {
            return _queryableCast ??= new MethodFinder(typeof(Queryable),
                    nameof(Queryable.Cast),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(IQueryable) }
                }
               .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("System.Linq.Queryable.Cast()"));
        }

        public static MethodInfo Like()
        {
            return _like ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.Like),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(string), typeof(string) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.Like()"));
        }

        public static MethodInfo IsNull()
        {
            return _isNull ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.IsNull),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.IsNull()"));
        }

        public static MethodInfo IsNotNull()
        {
            return _isNotNull ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.IsNotNull),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    ArgumentTypes = new[] { typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SqlExpressionsExtensions.IsNotNull()"));
        }

        public static MethodInfo Assign()
        {
            return _assign ??= new MethodFinder(typeof(SqlExpressionsExtensions),
                    nameof(SqlExpressionsExtensions.Assign),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
                {
                    TypeArguments = new[] { typeof(object) },
                    ArgumentTypes = new[] { typeof(object), typeof(object) }
                }
                .FindMethod() ?? throw new InvalidOperationException(CouldNotFindMethodFormat.Format("SpaceEngineers.Core.DataAccess.Orm.Linq.SqlExpressionsExtensions.Assign()"));
        }
    }
}