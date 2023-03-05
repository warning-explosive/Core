namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class Repository : IRepository,
                                IResolvable<IRepository>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IAsyncQueryProvider _queryProvider;

        public Repository(
            IDependencyContainer dependencyContainer,
            IAsyncQueryProvider queryProvider)
        {
            _dependencyContainer = dependencyContainer;
            _queryProvider = queryProvider;
        }

        public IQueryable<TEntity> All<TEntity>()
            where TEntity : IUniqueIdentified
        {
            var expression = Expression.Call(
                Expression.Constant(this),
                LinqMethods.RepositoryAll().MakeGenericMethod(typeof(TEntity)));

            return _queryProvider.CreateQuery<TEntity>(expression);
        }

        public Task<TEntity> Single<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return SingleByPrimaryKey<TEntity, TKey>(key)
                .CachedExpression("E9FF5F3C-13C0-4269-9BAE-7064E8681EBE")
                .SingleAsync(token);
        }

        public Task<TEntity?> SingleOrDefault<TEntity, TKey>(TKey key, CancellationToken token)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            return SingleByPrimaryKey<TEntity, TKey>(key)
                .CachedExpression("C73644BD-4C73-43CC-984D-462CB39DADA2")
                .SingleOrDefaultAsync(token);
        }

        public IInsertQueryable<IDatabaseEntity> Insert(
            IDatabaseContext transaction,
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior)
        {
            if (!entities.Any())
            {
                throw new InvalidOperationException("Entities are empty");
            }

            var expression = Expression.Call(
                null,
                LinqMethods.WithDependencyContainer(),
                Expression.Call(
                    Expression.Constant(this),
                    LinqMethods.RepositoryInsert(),
                    Expression.Constant(transaction),
                    Expression.Constant(entities),
                    Expression.Constant(insertBehavior)),
                Expression.Constant(_dependencyContainer));

            return (IInsertQueryable<IDatabaseEntity>)_queryProvider.CreateQuery<IDatabaseEntity>(expression);
        }

        public IUpdateQueryable<TEntity> Update<TEntity>(IDatabaseContext transaction)
            where TEntity : IDatabaseEntity
        {
            var expression = Expression.Call(
                Expression.Constant(this),
                LinqMethods.RepositoryUpdate().MakeGenericMethod(typeof(TEntity)),
                Expression.Constant(transaction));

            return (IUpdateQueryable<TEntity>)_queryProvider.CreateQuery<TEntity>(expression);
        }

        public IDeleteQueryable<TEntity> Delete<TEntity>(IDatabaseContext transaction)
            where TEntity : IDatabaseEntity
        {
            var expression = Expression.Call(
                Expression.Constant(this),
                LinqMethods.RepositoryDelete().MakeGenericMethod(typeof(TEntity)),
                Expression.Constant(transaction));

            return (IDeleteQueryable<TEntity>)_queryProvider.CreateQuery<TEntity>(expression);
        }

        private IQueryable<TEntity> SingleByPrimaryKey<TEntity, TKey>(TKey key)
            where TEntity : IUniqueIdentified
            where TKey : notnull
        {
            var expression = Expression.Call(
                null,
                LinqMethods.QueryableWhere().MakeGenericMethod(typeof(TEntity)),
                Expression.Call(
                    Expression.Constant(this),
                    LinqMethods.RepositoryAll().MakeGenericMethod(typeof(TEntity))),
                Predicate(key));

            return _queryProvider.CreateQuery<TEntity>(expression);

            static Expression<Func<TEntity, bool>> Predicate(TKey key)
            {
                return entity => Equals(entity.PrimaryKey, key);
            }
        }
    }
}