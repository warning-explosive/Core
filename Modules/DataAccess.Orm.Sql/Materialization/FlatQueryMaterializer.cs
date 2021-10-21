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
    using Orm.Settings;
    using Translation;
    using Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryMaterializer<T> : IQueryMaterializer<FlatQuery, T>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IObjectBuilder _objectBuilder;

        public FlatQueryMaterializer(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction transaction,
            IObjectBuilder objectBuilder)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _transaction = transaction;
            _objectBuilder = objectBuilder;
        }

        public async Task<T> MaterializeScalar(FlatQuery query, CancellationToken token)
        {
            var dynamicResult = await GetDynamicResult(query, token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            if (dynamicResult.Any())
            {
                var dynamicValues = dynamicResult.SingleOrDefault();

                var values = dynamicValues as IDictionary<string, object>;
                var built = _objectBuilder.Build(typeof(T), values);

                return (T)built!;
            }

            return default!;
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
            var ormSettings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var command = new CommandDefinition(
                InlineQueryParameters(query),
                null,
                _transaction.UnderlyingDbTransaction,
                ormSettings.QueryTimeout.Seconds,
                CommandType.Text,
                CommandFlags.Buffered,
                token);

            return await _transaction
                .UnderlyingDbTransaction
                .Connection
                .QueryAsync(command)
                .ConfigureAwait(false);
        }

        private string InlineQueryParameters(FlatQuery query)
        {
            var sqlQuery = query.Query;

            foreach (var (name, value) in query.QueryParameters)
            {
                if (!query.Query.Contains($"@{name}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                sqlQuery = sqlQuery.Replace($"@{name}", value.QueryParameterSqlExpression(_dependencyContainer), StringComparison.OrdinalIgnoreCase);
            }

            return sqlQuery;
        }
    }
}