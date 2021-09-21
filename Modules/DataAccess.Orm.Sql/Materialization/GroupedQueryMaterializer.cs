namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Linq;
    using Orm.Linq;
    using Translation;

    [Component(EnLifestyle.Scoped)]
    internal class GroupedQueryMaterializer<TKey, TValue> : IQueryMaterializer<GroupedQuery, IGrouping<TKey, TValue>>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IQueryMaterializer<FlatQuery, TKey> _keysQueryMaterializer;
        private readonly IQueryMaterializer<FlatQuery, TValue> _valuesQueryMaterializer;

        public GroupedQueryMaterializer(
            IDependencyContainer dependencyContainer,
            IQueryMaterializer<FlatQuery, TKey> keysQueryMaterializer,
            IQueryMaterializer<FlatQuery, TValue> valuesQueryMaterializer)
        {
            _dependencyContainer = dependencyContainer;
            _keysQueryMaterializer = keysQueryMaterializer;
            _valuesQueryMaterializer = valuesQueryMaterializer;
        }

        public async Task<IGrouping<TKey, TValue>> MaterializeScalar(GroupedQuery query, CancellationToken token)
        {
            var key = await MaterializeScalarKey(query, token).ConfigureAwait(false);
            var values = MaterializeValues(key, query, token).AsEnumerable(token);

            return new Grouping<TKey, TValue>(key, values);
        }

        public async IAsyncEnumerable<IGrouping<TKey, TValue>> Materialize(GroupedQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var key in MaterializeKeys(query, token))
            {
                var values = MaterializeValues(key, query, token).AsEnumerable(token);

                yield return new Grouping<TKey, TValue>(key, values);
            }
        }

        private Task<TKey> MaterializeScalarKey(GroupedQuery query, CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);

            return _keysQueryMaterializer.MaterializeScalar(keysQuery, token);
        }

        private IAsyncEnumerable<TKey> MaterializeKeys(GroupedQuery query, CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);

            return _keysQueryMaterializer.Materialize(keysQuery, token);
        }

        private async IAsyncEnumerable<TValue> MaterializeValues(TKey key, GroupedQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var keyValues = key.GetQueryParametersValues();

            var valuesExpression = query.ValuesExpressionProducer(keyValues);

            var valuesQuery = await valuesExpression.Translate(_dependencyContainer, 0, token).ConfigureAwait(false);
            var valuesQueryParameters = valuesExpression.ExtractQueryParameters(_dependencyContainer);

            await foreach (var item in _valuesQueryMaterializer.Materialize(new FlatQuery(valuesQuery, valuesQueryParameters), token))
            {
                yield return item;
            }
        }
    }
}