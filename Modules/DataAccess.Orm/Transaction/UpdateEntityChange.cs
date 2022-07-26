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
    using Microsoft.Extensions.Logging;

    internal class UpdateEntityChange<TEntity, TKey> : ITransactionalChange
        where TEntity : IDatabaseEntity<TKey>
        where TKey : notnull
    {
        private readonly long _version;
        private readonly long _affectedRowsCount;
        private readonly IReadOnlyCollection<UpdateInfo<TEntity, TKey>> _infos;
        private readonly Expression<Func<TEntity, bool>> _predicate;
        private readonly long _updateVersion;

        public UpdateEntityChange(
            long version,
            long affectedRowsCount,
            IReadOnlyCollection<UpdateInfo<TEntity, TKey>> infos,
            Expression<Func<TEntity, bool>> predicate,
            long updateVersion)
        {
            _version = version;
            _affectedRowsCount = affectedRowsCount;
            _infos = infos;
            _predicate = predicate;
            _updateVersion = updateVersion;
        }

        public async Task Apply(
            IAdvancedDatabaseTransaction databaseTransaction,
            ILogger logger,
            CancellationToken token)
        {
            var infos = _infos
               .Concat(new[] { new UpdateInfo<TEntity, TKey>(entity => entity.Version, _ => _updateVersion) })
               .ToList();

            var actualAffectedRowsCount = await databaseTransaction
               .Write<TEntity, TKey>()
               .Update(infos, _predicate.And(entity => entity.Version == _version), token)
               .ConfigureAwait(false);

            if (actualAffectedRowsCount != _affectedRowsCount)
            {
                throw new DatabaseConcurrentUpdateException(typeof(TEntity));
            }
        }
    }
}