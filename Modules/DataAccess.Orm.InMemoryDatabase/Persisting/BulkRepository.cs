namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Scoped)]
    internal class BulkRepository<TEntity, TKey> : IBulkRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly IRepository<TEntity, TKey> _repository;

        public BulkRepository(IRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }

        public Task Insert(IEnumerable<TEntity> entities, CancellationToken token)
        {
            return entities
                .Select(entity => _repository.Insert(entity, token))
                .WhenAll();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKeys, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token)
        {
            return primaryKeys
                .Select(primaryKey => _repository.Update(primaryKey, accessor, value, token))
                .WhenAll();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKeys, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token)
        {
            return primaryKeys
                .Select(primaryKey => _repository.Update(primaryKey, accessor, valueProducer, token))
                .WhenAll();
        }

        public Task Delete(IEnumerable<TKey> primaryKeys, CancellationToken token)
        {
            return primaryKeys
                .Select(primaryKey => _repository.Delete(primaryKey, token))
                .WhenAll();
        }
    }
}