namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class BulkRepository<TEntity, TKey> : IBulkRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        public Task Insert(IEnumerable<TEntity> entities, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKeys, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(IEnumerable<TKey> primaryKeys, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IEnumerable<TKey> primaryKeys, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}