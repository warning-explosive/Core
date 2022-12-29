namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Linq;
    using Translation;
    using Translation.Extensions;

    [Component(EnLifestyle.Singleton)]
    internal class GroupedQueryMaterializer<TKey, TValue> : IQueryMaterializer<GroupedQuery, IGrouping<TKey, TValue>>,
                                                            IResolvable<IQueryMaterializer<GroupedQuery, IGrouping<TKey, TValue>>>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;
        private readonly IQueryMaterializer<FlatQuery, TKey> _keysQueryMaterializer;
        private readonly IQueryMaterializer<FlatQuery, TValue> _valuesQueryMaterializer;

        public GroupedQueryMaterializer(
            ISqlExpressionTranslatorComposite sqlExpressionTranslator,
            IQueryMaterializer<FlatQuery, TKey> keysQueryMaterializer,
            IQueryMaterializer<FlatQuery, TValue> valuesQueryMaterializer)
        {
            _sqlExpressionTranslator = sqlExpressionTranslator;
            _keysQueryMaterializer = keysQueryMaterializer;
            _valuesQueryMaterializer = valuesQueryMaterializer;
        }

        public async Task<IGrouping<TKey, TValue>> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            CancellationToken token)
        {
            var key = await MaterializeScalarKey(transaction, query, token).ConfigureAwait(false);
            var values = MaterializeValues(transaction, key, query, token).AsEnumerable(token);

            return new Grouping<TKey, TValue>(key, values);
        }

        public async IAsyncEnumerable<IGrouping<TKey, TValue>> Materialize(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var key in MaterializeKeys(transaction, query, token))
            {
                var values = MaterializeValues(transaction, key, query, token).AsEnumerable(token);

                yield return new Grouping<TKey, TValue>(key, values);
            }
        }

        private Task<TKey> MaterializeScalarKey(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);

            return _keysQueryMaterializer.MaterializeScalar(transaction, keysQuery, token);
        }

        private IAsyncEnumerable<TKey> MaterializeKeys(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);

            return _keysQueryMaterializer.Materialize(transaction, keysQuery, token);
        }

        private async IAsyncEnumerable<TValue> MaterializeValues(
            IAdvancedDatabaseTransaction transaction,
            TKey key,
            GroupedQuery query,
            [EnumeratorCancellation] CancellationToken token)
        {
            var keyValues = key.AsQueryParametersValues();

            var valuesExpression = query.ValuesExpressionProducer(keyValues);

            var valuesQuery = _sqlExpressionTranslator.Translate(valuesExpression, 0);
            var valuesQueryParameters = valuesExpression.ExtractQueryParameters();
            var valuesFlatQuery = new FlatQuery(valuesQuery, valuesQueryParameters);

            await foreach (var item in _valuesQueryMaterializer.Materialize(transaction, valuesFlatQuery, token))
            {
                yield return item;
            }
        }
    }
}