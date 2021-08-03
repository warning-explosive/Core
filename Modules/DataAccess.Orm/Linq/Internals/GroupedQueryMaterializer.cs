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
        private readonly IQueryMaterializer<FlatQuery, TKey> _keyQueryMaterializer;
        private readonly IQueryMaterializer<FlatQuery, TValue> _valueQueryMaterializer;

        public GroupedQueryMaterializer(
            IQueryMaterializer<FlatQuery, TKey> keyQueryMaterializer,
            IQueryMaterializer<FlatQuery, TValue> valueQueryMaterializer)
        {
            _keyQueryMaterializer = keyQueryMaterializer;
            _valueQueryMaterializer = valueQueryMaterializer;
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
            var keyQuery = new FlatQuery(query.KeyQuery, query.KeyQueryParameters);

            return _keyQueryMaterializer.Materialize(keyQuery, token);
        }

        private IAsyncEnumerable<TValue> MaterializeValues(TKey key, GroupedQuery query, CancellationToken token)
        {
            /* TODO: apply key as parameter */

            var valueQuery = new FlatQuery(query.ValueQuery, query.ValueQueryParameters);

            return _valueQueryMaterializer.Materialize(valueQuery, token);
        }
    }
}