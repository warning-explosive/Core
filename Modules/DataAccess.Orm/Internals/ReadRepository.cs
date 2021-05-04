namespace SpaceEngineers.Core.DataAccess.Orm.Internals
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using GenericDomain.Abstractions;
    using Settings;
    using SettingsManager.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class ReadRepository<TAggregate> : IReadRepository<TAggregate>
        where TAggregate : class, IAggregate
    {
        private readonly IDatabaseTransaction _transaction;
        private readonly IQueryBuilder<TAggregate> _queryBuilder;
        private readonly IObjectBuilder<TAggregate> _objectBuilder;
        private readonly ISettingsManager<OrmSettings> _ormSettingsProvider;

        public ReadRepository(
            IDatabaseTransaction transaction,
            IQueryBuilder<TAggregate> queryBuilder,
            IObjectBuilder<TAggregate> objectBuilder,
            ISettingsManager<OrmSettings> ormSettingsProvider)
        {
            _transaction = transaction;
            _queryBuilder = queryBuilder;
            _objectBuilder = objectBuilder;
            _ormSettingsProvider = ormSettingsProvider;
        }

        public async Task<IEnumerable<TAggregate>> ReadAll(IQueryable<TAggregate> query)
        {
            var (databaseQuery, parameters) = _queryBuilder.BuildQuery(query);

            var ormSettings = await _ormSettingsProvider
                .Get()
                .ConfigureAwait(false);

            var transaction = await _transaction
                .Open()
                .ConfigureAwait(false);

            var dynamicResult = await transaction
                .Connection
                .QueryAsync(databaseQuery, parameters, transaction, ormSettings.QueryTimeout.Seconds, CommandType.Text)
                .ConfigureAwait(false);

            return dynamicResult
                .Select(row => (row as IDictionary<string, object>) !)
                .Select(_objectBuilder.Build);
        }
    }
}