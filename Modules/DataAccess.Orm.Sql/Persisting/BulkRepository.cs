namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.DatabaseEntity;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class BulkRepository<TEntity, TKey> : IBulkRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
    {
        public Task Insert(IEnumerable<TEntity> entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TEntity> entity, Func<TEntity, TValue> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TEntity> entity, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKey, Func<TEntity, TValue> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKey, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IEnumerable<TEntity> entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IEnumerable<TKey> primaryKey, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}