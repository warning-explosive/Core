namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Transaction;

    internal class DeleteEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private readonly TKey _primaryKey;
        private readonly long _version;

        public DeleteEntityChange(TKey primaryKey, long version)
        {
            _primaryKey = primaryKey;
            _version = version;
        }

        public Task Apply(IDatabaseContext databaseContext, CancellationToken token)
        {
            return databaseContext
               .Write<TEntity, TKey>()
               .Delete(new[] { _primaryKey }, token);
        }
    }
}