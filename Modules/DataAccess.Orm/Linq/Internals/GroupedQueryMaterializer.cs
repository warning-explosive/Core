namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

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
                var values = MaterializeValues(key, query, token).AsEnumerable(token);

                yield return new Grouping<TKey, TValue>(key, values);
            }
        }

        private IAsyncEnumerable<TKey> MaterializeKeys(GroupedQuery query, CancellationToken token)
        {
            var keyQuery = new FlatQuery(query.KeyQuery, query.KeyQueryParameters);

            return _keyQueryMaterializer.Materialize(keyQuery, token);
        }

        private IAsyncEnumerable<TValue> MaterializeValues(TKey key, GroupedQuery query, CancellationToken token)
        {
            var valueQueryParameters = ApplyKeyValues(key, query.ValueQueryParameters);

            var valueQuery = new FlatQuery(query.ValueQuery, valueQueryParameters);

            return _valueQueryMaterializer.Materialize(valueQuery, token);
        }

        private static object? ApplyKeyValues(TKey key, object? queryValueQueryParameters)
        {
            IReadOnlyDictionary<string, object?> values = typeof(TKey).IsPrimitive()
                ? new Dictionary<string, object?> { [string.Format(TranslationContext.QueryParameterFormat, 0)] = key }
                : key?.ToPropertyDictionary() ?? new Dictionary<string, object?>();

            if (queryValueQueryParameters == null)
            {
                return null;
            }

            queryValueQueryParameters
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                .Join(values,
                    property => property.Name,
                    value => value.Key,
                    (property, value) => (property, value.Value),
                    StringComparer.OrdinalIgnoreCase)
                .Each(pair => pair.property.SetValue(queryValueQueryParameters, pair.Value));

            return queryValueQueryParameters;
        }
    }
}