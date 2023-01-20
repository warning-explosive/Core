namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
    internal class GroupedQueryMaterializer : IQueryMaterializer,
                                              IQueryMaterializer<GroupedQuery>,
                                              IResolvable<IQueryMaterializer<GroupedQuery>>,
                                              ICollectionResolvable<IQueryMaterializer>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;
        private readonly IQueryMaterializer<FlatQuery> _flatQueryMaterializer;

        private ConcurrentDictionary<Type, MethodInfo> _cctors;

        public GroupedQueryMaterializer(
            ISqlExpressionTranslatorComposite sqlExpressionTranslator,
            IQueryMaterializer<FlatQuery> flatQueryMaterializer)
        {
            _sqlExpressionTranslator = sqlExpressionTranslator;
            _flatQueryMaterializer = flatQueryMaterializer;

            _cctors = new ConcurrentDictionary<Type, MethodInfo>();
        }

        public Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token)
        {
            return query is GroupedQuery groupedQuery
                ? MaterializeScalar(transaction, groupedQuery, type, token)
                : throw new NotSupportedException($"Unsupported query type {query.GetType()}");
        }

        public IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token)
        {
            return query is GroupedQuery groupedQuery
                ? Materialize(transaction, groupedQuery, type, token)
                : throw new NotSupportedException($"Unsupported query type {query.GetType()}");
        }

        public async Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            Type type,
            CancellationToken token)
        {
            var key = await MaterializeScalarKey(transaction, query, type, token).ConfigureAwait(false);

            var values = await MaterializeValues(transaction, key, query, type, token)
                .AsEnumerable(token)
                .ConfigureAwait(false);

            return CreateGroup(type, key, values);
        }

        public async IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            Type type,
            [EnumeratorCancellation] CancellationToken token)
        {
            var source = MaterializeKeys(transaction, query, type, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var key in source)
            {
                var values = await MaterializeValues(transaction, key, query, type, token)
                    .AsEnumerable(token)
                    .ConfigureAwait(false);

                yield return CreateGroup(type, key, values);
            }
        }

        private Task<object?> MaterializeScalarKey(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            Type type,
            CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);
            var keyType = type.ExtractGenericArgumentAt(typeof(IGrouping<,>));

            return _flatQueryMaterializer.MaterializeScalar(transaction, keysQuery, keyType, token);
        }

        private IAsyncEnumerable<object?> MaterializeKeys(
            IAdvancedDatabaseTransaction transaction,
            GroupedQuery query,
            Type type,
            CancellationToken token)
        {
            var keysQuery = new FlatQuery(query.KeysQuery, query.KeysQueryParameters);
            var keyType = type.ExtractGenericArgumentAt(typeof(IGrouping<,>));

            return _flatQueryMaterializer.Materialize(transaction, keysQuery, keyType, token);
        }

        private async IAsyncEnumerable<object?> MaterializeValues(
            IAdvancedDatabaseTransaction transaction,
            object? key,
            GroupedQuery query,
            Type type,
            [EnumeratorCancellation] CancellationToken token)
        {
            var keyValues = key.AsQueryParametersValues();

            var valuesExpression = query.ValuesExpressionProducer(keyValues);

            var valuesQuery = _sqlExpressionTranslator.Translate(valuesExpression, 0);
            var valuesQueryParameters = valuesExpression.ExtractQueryParameters();
            var valuesFlatQuery = new FlatQuery(valuesQuery, valuesQueryParameters);

            var valueType = type.ExtractGenericArgumentAt(typeof(IGrouping<,>), 1);

            var source = _flatQueryMaterializer
                .Materialize(transaction, valuesFlatQuery, valueType, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var item in source)
            {
                yield return item;
            }
        }

        private object? CreateGroup(Type type, object? key, IEnumerable<object?> values)
        {
            return _cctors
                .GetOrAdd(type, GetGroupCctor)
                .Invoke(null, new[] { key, values });

            static MethodInfo GetGroupCctor(Type groupType)
            {
                var leftKey = groupType.ExtractGenericArgumentAt(typeof(IGrouping<,>), 0);
                var rightKey = groupType.ExtractGenericArgumentAt(typeof(IGrouping<,>), 1);

                return new MethodFinder(
                        typeof(GroupedQueryMaterializer),
                        nameof(CreateGroupInstance),
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
                    {
                        TypeArguments = new[] { leftKey, rightKey },
                        ArgumentTypes = new[] { leftKey, typeof(IEnumerable<>).MakeGenericType(typeof(object)) }
                    }
                    .FindMethod()
                    .EnsureNotNull($"Could not find {nameof(CreateGroupInstance)} method")
                    .MakeGenericMethod(leftKey, rightKey);
            }
        }

        private static IGrouping<TKey, TValue> CreateGroupInstance<TKey, TValue>(TKey key, IEnumerable<object?> values)
        {
            return new Grouping<TKey, TValue>(key, values.Cast<TValue>());
        }
    }
}