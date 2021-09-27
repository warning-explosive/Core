namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Persisting
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly IAdvancedDatabaseTransaction _transaction;

        public Repository(IAdvancedDatabaseTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task Insert(TEntity entity, CancellationToken token)
        {
            return _transaction
                .UnderlyingDbTransaction
                .Insert<TEntity, TKey>(entity, token);
        }

        public async Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token)
        {
            var entity = await _transaction
                .UnderlyingDbTransaction
                .Single<TEntity, TKey>(primaryKey, token)
                .ConfigureAwait(false);

            var updated = UpdateRecord(entity, accessor, value);

            await _transaction
                .UnderlyingDbTransaction
                .Update<TEntity, TKey, TValue>(updated, token)
                .ConfigureAwait(false);
        }

        public async Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token)
        {
            var entity = await _transaction
                .UnderlyingDbTransaction
                .Single<TEntity, TKey>(primaryKey, token)
                .ConfigureAwait(false);

            var updated = UpdateRecord(entity, accessor, valueProducer.Compile().Invoke(entity));

            await _transaction
                .UnderlyingDbTransaction
                .Update<TEntity, TKey, TValue>(updated, token)
                .ConfigureAwait(false);
        }

        public Task Delete(TKey primaryKey, CancellationToken token)
        {
            return _transaction
                .UnderlyingDbTransaction
                .Delete<TEntity, TKey>(primaryKey, token);
        }

        private static TEntity UpdateRecord<TValue>(TEntity entity, Expression<Func<TEntity, TValue>> accessor, TValue value)
        {
            var visitor = new FindPropertyAccessExpressionVisitor();
            visitor.Visit(accessor);

            var copy = entity
                .CallMethod("<Clone>$")
                .Invoke<TEntity>();

            visitor
                .PropertyInfo
                .SetValue(copy, value);

            return copy;
        }
    }
}