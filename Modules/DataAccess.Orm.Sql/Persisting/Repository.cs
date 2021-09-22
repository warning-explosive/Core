namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Persisting
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.DatabaseEntity;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
    {
        public Task Insert(TEntity entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TEntity entity, Func<TEntity, TValue> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TEntity entity, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TKey primaryKey, Func<TEntity, TValue> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TKey primaryKey, Func<TEntity, TValue> accessor, Func<TEntity, TValue> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TEntity entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TKey primaryKey, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}