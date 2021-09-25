namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Persisting
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
        public Task Insert(TEntity entity, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, TValue value, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Update<TValue>(TKey primaryKey, Expression<Func<TEntity, TValue>> accessor, Expression<Func<TEntity, TValue>> valueProducer, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Delete(TKey primaryKey, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}