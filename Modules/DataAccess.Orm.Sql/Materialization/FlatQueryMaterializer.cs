namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Orm.Linq;
    using Settings;
    using Translation;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryMaterializer<T> : IQueryMaterializer<FlatQuery, T>
    {
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IObjectBuilder _objectBuilder;

        public FlatQueryMaterializer(
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IAdvancedDatabaseTransaction transaction,
            IObjectBuilder objectBuilder)
        {
            _ormSettingsProvider = ormSettingsProvider;
            _transaction = transaction;
            _objectBuilder = objectBuilder;
        }

        public async Task<T> MaterializeScalar(FlatQuery query, CancellationToken token)
        {
            var dynamicResult = await GetDynamicResult(query, token).ConfigureAwait(false);
            var dynamicValues = dynamicResult?.SingleOrDefault();

            token.ThrowIfCancellationRequested();

            var values = dynamicValues as IDictionary<string, object>;
            var built = _objectBuilder.Build(typeof(T), values);

            return (T)built!;
        }

        public async IAsyncEnumerable<T> Materialize(FlatQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var dynamicResult = await GetDynamicResult(query, token).ConfigureAwait(false);

            foreach (var dynamicValues in dynamicResult)
            {
                token.ThrowIfCancellationRequested();

                var values = dynamicValues as IDictionary<string, object>;
                var built = _objectBuilder.Build(typeof(T), values);

                yield return (T)built!;
            }
        }

        private async Task<IEnumerable<dynamic>?> GetDynamicResult(FlatQuery query, CancellationToken token)
        {
            var ormSettings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var transaction = _transaction.UnderlyingDbTransaction;

            return await transaction
                .Connection
                .QueryAsync(query.Query, query.QueryParameters, transaction, ormSettings.QueryTimeout.Seconds, CommandType.Text)
                .ConfigureAwait(false);
        }
    }
}