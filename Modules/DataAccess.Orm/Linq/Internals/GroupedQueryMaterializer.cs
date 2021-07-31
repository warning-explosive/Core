namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class GroupedQueryMaterializer<TKey, TValue> : IQueryMaterializer<GroupedQuery, IGrouping<TKey, TValue>>
    {
        private readonly IQueryMaterializer<FlatQuery, TKey> _keysQueryMaterializer;
        private readonly IQueryMaterializer<FlatQuery, TValue> _valuesQueryMaterializer;

        public GroupedQueryMaterializer(
            IQueryMaterializer<FlatQuery, TKey> keysQueryMaterializer,
            IQueryMaterializer<FlatQuery, TValue> valuesQueryMaterializer)
        {
            _keysQueryMaterializer = keysQueryMaterializer;
            _valuesQueryMaterializer = valuesQueryMaterializer;
        }

        public async IAsyncEnumerable<IGrouping<TKey, TValue>> Materialize(GroupedQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var key in MaterializeKeys(query, token))
            {
                /* TODO: create group with key and values */
                var values = MaterializeValues(key, query, token);
            }

            yield break;
        }

        private IAsyncEnumerable<TKey> MaterializeKeys(GroupedQuery query, CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery)
            {
                Parameters = query.KeysParameters
            };

            return _keysQueryMaterializer.Materialize(keysQuery, token);
        }

        private IAsyncEnumerable<TValue> MaterializeValues(TKey key, GroupedQuery query, CancellationToken token)
        {
            /* TODO: apply key as parameter */

            var valuesQuery = new FlatQuery(query.ValuesQuery)
            {
                Parameters = query.ValuesParameters
            };

            return _valuesQueryMaterializer.Materialize(valuesQuery, token);
        }
    }
}