namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly IBulkRepository<TEntity, TKey> _bulkRepository;

        public Repository(IBulkRepository<TEntity, TKey> bulkRepository)
        {
            _bulkRepository = bulkRepository;
        }

        public Task Insert(
            TEntity entity,
            CancellationToken token)
        {
            return _bulkRepository.Insert(new[] { entity }, token);
        }

        public Task Update<TValue>(
            TKey primaryKey,
            Expression<Func<TEntity, TValue>> accessor,
            TValue value,
            CancellationToken token)
        {
            return _bulkRepository.Update(new[] { primaryKey }, accessor, value, token);
        }

        public Task Update<TValue>(
            TKey primaryKey,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            CancellationToken token)
        {
            return _bulkRepository.Update(new[] { primaryKey }, accessor, valueProducer, token);
        }

        public Task Delete(
            TKey primaryKey,
            CancellationToken token)
        {
            return _bulkRepository.Delete(new[] { primaryKey }, token);
        }
    }
}