namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Dapper;
    using Orm.Linq;
    using Settings;
    using Translation;
    using Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryMaterializer<T> : IQueryMaterializer<FlatQuery, T>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _ormSettingsProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IObjectBuilder _objectBuilder;

        public FlatQueryMaterializer(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IAdvancedDatabaseTransaction transaction,
            IObjectBuilder objectBuilder)
        {
            _dependencyContainer = dependencyContainer;
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

            var sqlQuery = InlineQueryParameters(query);

            return await transaction
                .Connection
                .QueryAsync(sqlQuery, null, transaction, ormSettings.QueryTimeout.Seconds, CommandType.Text)
                .ConfigureAwait(false);
        }

        private string InlineQueryParameters(FlatQuery query)
        {
            var sqlQuery = query.Query;

            foreach (var (name, (type, value)) in query.QueryParameters)
            {
                if (!query.Query.Contains($"@{name}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sqlQuery = sqlQuery.Replace($"@{name}", value.QueryParameterSqlExpression(type, _dependencyContainer), StringComparison.OrdinalIgnoreCase);
            }

            return sqlQuery;
        }
    }
}