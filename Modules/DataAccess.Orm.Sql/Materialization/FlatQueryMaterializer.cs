namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Api.Abstractions;
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
        private readonly IDatabaseTransaction _transaction;
        private readonly IObjectBuilder _objectBuilder;

        public FlatQueryMaterializer(
            ISettingsProvider<OrmSettings> ormSettingsProvider,
            IDatabaseTransaction transaction,
            IObjectBuilder objectBuilder)
        {
            _ormSettingsProvider = ormSettingsProvider;
            _transaction = transaction;
            _objectBuilder = objectBuilder;
        }

        public async IAsyncEnumerable<T> Materialize(FlatQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var ormSettings = await _ormSettingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var transaction = await _transaction
                .Open(token)
                .ConfigureAwait(false);

            var dynamicResult = await transaction
                .Connection
                .QueryAsync(query.Query, query.QueryParameters, transaction, ormSettings.QueryTimeout.Seconds, CommandType.Text)
                .ConfigureAwait(false);

            foreach (var dynamicValues in dynamicResult)
            {
                token.ThrowIfCancellationRequested();

                var values = dynamicValues as IDictionary<string, object>;
                var built = _objectBuilder.Build(typeof(T), values);

                if (built != null)
                {
                    yield return (T)built;
                }
            }
        }
    }
}