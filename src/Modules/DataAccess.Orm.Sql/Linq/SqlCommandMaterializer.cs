namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Connection;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.ObjectBuilder;
    using Model;
    using Transaction;
    using Translation;

    [Component(EnLifestyle.Singleton)]
    internal class SqlCommandMaterializer : ICommandMaterializer,
                                            ICommandMaterializer<SqlCommand>,
                                            IResolvable<ICommandMaterializer<SqlCommand>>,
                                            ICollectionResolvable<ICommandMaterializer>
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IObjectBuilder _objectBuilder;

        public SqlCommandMaterializer(
            IDatabaseConnectionProvider connectionProvider,
            IModelProvider modelProvider,
            IJsonSerializer jsonSerializer,
            IObjectBuilder objectBuilder)
        {
            _connectionProvider = connectionProvider;
            _modelProvider = modelProvider;
            _jsonSerializer = jsonSerializer;
            _objectBuilder = objectBuilder;
        }

        public IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            CancellationToken token)
        {
            if (command is not SqlCommand sqlCommand)
            {
                throw new NotSupportedException($"Unsupported command type {command.GetType()}");
            }

            return Materialize<T>(transaction, sqlCommand, token);
        }

        public IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            SqlCommand command,
            CancellationToken token)
        {
            return MaterializeInternal<T>(transaction, command, token);
        }

        private async IAsyncEnumerable<T> MaterializeInternal<T>(
            IAdvancedDatabaseTransaction transaction,
            SqlCommand command,
            [EnumeratorCancellation] CancellationToken token)
        {
            var asyncSource = _connectionProvider
                .Query<T>(transaction, command, token)
                .WithCancellation(token)
                .ConfigureAwait(false);

            await foreach (var values in asyncSource)
            {
                var item = await MaterializeInternal(transaction, typeof(T), values, token).ConfigureAwait(false);

                yield return (T)item !;
            }
        }

        private async Task<object?> MaterializeInternal(
            IAdvancedDatabaseTransaction transaction,
            Type type,
            IDictionary<string, object?> values,
            CancellationToken token)
        {
            MaterializeRelations(transaction, type, values);

            var multipleRelationValues = InitializeMultipleRelations(type, values);

            var arrangedValues = ArrangeValues(type, values);

            object? built;

            if (type.IsDatabaseEntity()
                && TryGetValueFromTransaction(transaction, type, arrangedValues[nameof(IUniqueIdentified.PrimaryKey)] !, out var stored))
            {
                // TODO: why refill from query results?
                _objectBuilder.Fill(type, stored, arrangedValues);
                built = stored;
            }
            else
            {
                built = _objectBuilder.Build(type, arrangedValues) !;
            }

            StoreInTransaction(transaction, built);

            await MaterializeMultipleRelations(transaction, built, type, multipleRelationValues, token).ConfigureAwait(false);

            return built;
        }

        private IDictionary<string, object?> ArrangeValues(Type type, IDictionary<string, object?> values)
        {
            {
                if (values.Count == 1
                    && values.Single() is var (key, value))
                {
                    if (TryBuildArray(type, key, value, _objectBuilder, out var built)
                        || TryBuildEnumFlags(type, key, value, _objectBuilder, out built)
                        || TryDeserializeJson(type, key, value, _jsonSerializer, out built))
                    {
                        values[key] = built;
                    }

                    return values;
                }
            }

            if (type.IsPrimitive())
            {
                return values;
            }

            {
                foreach (var (key, value) in values)
                {
                    var property = type.Column(key);

                    if (TryBuildArray(property.PropertyType, key, value, _objectBuilder, out var built)
                        || TryBuildEnumFlags(property.PropertyType, key, value, _objectBuilder, out built)
                        || TryDeserializeJson(property.PropertyType, key, value, _jsonSerializer, out built))
                    {
                        values[key] = built;
                    }
                }

                return values;
            }

            static bool TryBuildArray(
                Type type,
                string key,
                object? value,
                IObjectBuilder objectBuilder,
                out object? array)
            {
                if (value is not null or DBNull
                    && value.GetType().IsArray()
                    && type.IsDatabaseArray(out var elementType)
                    && elementType != null)
                {
                    var arrayValue = (Array)value;

                    var buffer = Array.CreateInstance(elementType, arrayValue.Length);

                    var enumerator = arrayValue.GetEnumerator();

                    var localValues = new Dictionary<string, object?>();

                    for (var i = 0; enumerator.MoveNext(); i++)
                    {
                        localValues[key] = enumerator.Current;

                        buffer.SetValue(objectBuilder.Build(elementType, localValues), i);
                    }

                    array = buffer;
                    return true;
                }

                array = null;
                return false;
            }

            static bool TryBuildEnumFlags(
                Type type,
                string key,
                object? value,
                IObjectBuilder objectBuilder,
                out object? enumFlags)
            {
                if (value is not null or DBNull
                    && value.GetType().IsArray()
                    && type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum)
                {
                    var arrayValue = (Array)value;

                    var enumerator = arrayValue.GetEnumerator();

                    var localValues = new Dictionary<string, object?>();

                    var enumFlagsValue = 0;

                    while (enumerator.MoveNext())
                    {
                        localValues[key] = enumerator.Current;

                        enumFlagsValue += (int)objectBuilder.Build(type, localValues) !;
                    }

                    enumFlags = enumFlagsValue;
                    return true;
                }

                enumFlags = null;
                return false;
            }

            static bool TryDeserializeJson(
                Type type,
                string key,
                object? value,
                IJsonSerializer jsonSerializer,
                out object? deserializedJsonObject)
            {
                if (value is string json
                    && type != typeof(string)
                    && !type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum
                    && !type.IsCompilerGenerated()
                    && !type.IsDatabaseEntity())
                {
                    deserializedJsonObject = jsonSerializer.DeserializeObject(json, type);
                    return true;
                }

                deserializedJsonObject = null;
                return false;
            }
        }

        private bool TryGetValueFromTransaction(
            IAdvancedDatabaseTransaction transaction,
            Type entity,
            object key,
            [NotNullWhen(true)] out object? stored)
        {
            stored = GetType()
               .CallMethod(nameof(TryGetValueFromTransaction))
               .WithTypeArguments(entity)
               .WithArguments(transaction.Store, key)
               .Invoke();

            return stored != default;
        }

        private static TEntity? TryGetValueFromTransaction<TEntity>(
            ITransactionalStore transactionalStore,
            object key)
            where TEntity : IUniqueIdentified
        {
            return transactionalStore.TryGetValue<TEntity>(key, out var stored)
                ? stored
                : default;
        }

        private static void StoreInTransaction(
            IAdvancedDatabaseTransaction transaction,
            object built)
        {
            if (built is IUniqueIdentified uniqueIdentified)
            {
                transaction.Store.Store(uniqueIdentified);
            }
        }

        private void MaterializeRelations(
            IAdvancedDatabaseTransaction transaction,
            Type type,
            IDictionary<string, object?> values)
        {
            var relations = _modelProvider
                .Columns(type)
                .Values
                .Where(column => column.IsRelation);

            foreach (var column in relations)
            {
                var primaryKey = values[column.Name];

                var relationKeys = values
                    .Select(pair => pair.Key)
                    .Where(key => key.StartsWith($"{column.Name}_"))
                    .ToList();

                var relationValues = new Dictionary<string, object?>();

                foreach (var relationKey in relationKeys)
                {
                    if (values.Remove(relationKey, out var value))
                    {
                        var cleanRelationKey = relationKey.Substring(column.Name.Length + 1);
                        relationValues[cleanRelationKey] = value;
                    }
                }

                values[column.Name] = column.Table.IsMtmTable
                    ? primaryKey
                    : MaterializeRelation(transaction, column.Relation.Target, primaryKey!, relationValues);
            }
        }

        private object MaterializeRelation(
            IAdvancedDatabaseTransaction transaction,
            Type type,
            object primaryKey,
            IDictionary<string, object?> values)
        {
            if (TryGetValueFromTransaction(transaction, type, primaryKey, out var relation))
            {
                return relation;
            }

            relation = _objectBuilder.Build(type, values) !;

            StoreInTransaction(transaction, relation);

            return relation;
        }

        private IReadOnlyDictionary<ColumnInfo, ICollection> InitializeMultipleRelations(
            Type type,
            IDictionary<string, object?> values)
        {
            return _modelProvider
                .Columns(type)
                .Values
                .Where(column => column.IsMultipleRelation)
                .ToDictionary(
                    column => column,
                    column =>
                    {
                        var value = (ICollection)Activator.CreateInstance(column.Relation.Target.ConstructMultipleRelationType()) !;

                        values[column.Name] = value;

                        return value;
                    });
        }

        private static Task MaterializeMultipleRelations(
            IAdvancedDatabaseTransaction transaction,
            object? built,
            Type type,
            IReadOnlyDictionary<ColumnInfo, ICollection> relationValues,
            CancellationToken token)
        {
            if (!relationValues.Any())
            {
                return Task.CompletedTask;
            }

            if (!type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                throw new InvalidOperationException($"Projection {type.FullName} should implement {nameof(IDatabaseEntity<object>)} so as to have multiple relation fields");
            }

            var keyType = type.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));

            return typeof(SqlCommandMaterializer)
                .CallMethod(nameof(MaterializeMultipleRelations))
                .WithTypeArguments(type, keyType)
                .WithArguments(transaction, built!, relationValues, token)
                .Invoke<Task>();
        }

        private static async Task MaterializeMultipleRelations<TEntity, TKey>(
            IAdvancedDatabaseTransaction transaction,
            TEntity built,
            IReadOnlyDictionary<ColumnInfo, ICollection> relationValues,
            CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            foreach (var (column, collection) in relationValues)
            {
                var mtmType = column.MultipleRelationTable!;
                var typeArguments = mtmType.ExtractGenericArguments(typeof(BaseMtmDatabaseEntity<,>));
                var leftKeyType = typeArguments[0];
                var rightKeyType = typeArguments[1];

                var ownerType = column.Table.Type;
                var collectionItemType = column.Relation.Target;

                await typeof(SqlCommandMaterializer)
                    .CallMethod(nameof(MaterializeMultipleRelation))
                    .WithTypeArguments(mtmType, ownerType, collectionItemType, leftKeyType, rightKeyType)
                    .WithArguments(transaction, built.PrimaryKey, collection, token)
                    .Invoke<Task>()
                    .ConfigureAwait(false);
            }
        }

        private static async Task MaterializeMultipleRelation<TMtm, TLeft, TRight, TLeftKey, TRightKey>(
            IAdvancedDatabaseTransaction transaction,
            TLeftKey ownerPrimaryKey,
            ICollection<TRight> collection,
            CancellationToken token)
            where TMtm : BaseMtmDatabaseEntity<TLeftKey, TRightKey>, IUniqueIdentified
            where TLeft : IDatabaseEntity<TLeftKey>
            where TRight : IDatabaseEntity<TRightKey>
            where TLeftKey : notnull
            where TRightKey : notnull
        {
            /*
             * select <fields>
             * from <collection_table> ct
             * where ct."PrimaryKey" in
             * (
             *      select mt."Right"
             *      from <mtm_table> mt
             *      where mt."Left" = <owner_primary_key>
             * )
             */

            var subQuery = transaction
                .All<TMtm>()
                .Where(mtm => Equals(mtm.Left, ownerPrimaryKey))
                .Select(mtm => mtm.Right);

            var items = await transaction
                .All<TRight>()
                .Where(databaseEntity => subQuery.Contains(databaseEntity.PrimaryKey))
                .CachedExpression($"{typeof(TMtm).Name}:9B70C4F3-989A-4609-A2E8-F1E16E243B72")
                .ToListAsync(token)
                .ConfigureAwait(false);

            items.Each(collection.Add);
        }
    }
}