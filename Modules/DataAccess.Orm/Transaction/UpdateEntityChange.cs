namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Transaction;

    internal class UpdateEntityChange<TEntity, TKey, TValue> : ITransactionalChange
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly TKey _primaryKey;
        private readonly long _version;
        private readonly Expression<Func<TEntity, TValue>> _accessor;
        private readonly Expression<Func<TEntity, TValue>> _valueProducer;

        public UpdateEntityChange(
            TKey primaryKey,
            long version,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer)
        {
            _primaryKey = primaryKey;
            _version = version;
            _accessor = accessor;
            _valueProducer = valueProducer;
        }

        public Task Apply(IDatabaseContext databaseContext, CancellationToken token)
        {
            return databaseContext
               .Write<TEntity, TKey>()
               .Update(new[] { _primaryKey }, _accessor, _valueProducer, token);
        }
    }
}