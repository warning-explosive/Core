namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Exceptions;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using Basics;

    internal class UpdateEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        private readonly long _version;
        private readonly long _expectedAffectedRowsCount;
        private readonly IReadOnlyCollection<UpdateInfo<TEntity, TKey>> _infos;
        private readonly Expression<Func<TEntity, bool>> _predicate;

        public UpdateEntityChange(
            long version,
            long expectedAffectedRowsCount,
            IReadOnlyCollection<UpdateInfo<TEntity, TKey>> infos,
            Expression<Func<TEntity, bool>> predicate)
        {
            _version = version;
            _expectedAffectedRowsCount = expectedAffectedRowsCount;
            _infos = infos;
            _predicate = predicate;
        }

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            CancellationToken token)
        {
            var infos = _infos
               .Concat(new[] { new UpdateInfo<TEntity, TKey>(entity => entity.Version, _ => _version) })
               .ToList();

            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity, TKey>()
               .Update(infos, _predicate.And(entity => entity.Version < _version), token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _expectedAffectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }
    }
}