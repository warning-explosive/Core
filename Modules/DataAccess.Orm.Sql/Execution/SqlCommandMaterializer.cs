namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Connection;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.ObjectBuilder;
    using Model;
    using Orm.Linq;
    using Orm.Model;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Model;
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
                .Query(transaction, command, token)
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
            var relationValues = ReplaceRelations(type, values);

            var multipleRelationValues = AddMultipleRelations(type, values);

            object? built;

            if (!type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>)))
            {
                built = Build(type, values) !;
            }
            else
            {
                if (TryGetValue(transaction, type, values[nameof(IUniqueIdentified.PrimaryKey)] !, out var stored))
                {
                    Fill(type, stored, values);
                    built = stored;
                }
                else
                {
                    built = Build(type, values) !;
                    Store(transaction, (IUniqueIdentified)built);
                }
            }

            await MaterializeRelations(transaction, built, relationValues, token).ConfigureAwait(false);

            await MaterializeMultipleRelations(transaction, built, type, multipleRelationValues, token).ConfigureAwait(false);

            return built;
        }

        private void Fill(Type type, object instance, IDictionary<string, object?> rawValues)
        {
            _objectBuilder.Fill(type, instance, ArrangeValues(type, rawValues));
        }

        private object? Build(Type type, IDictionary<string, object?> rawValues)
        {
            var values = ArrangeValues(type, rawValues);

            if (type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                && values.Count == 1
                && values.Single().Value == null)
            {
                return type.DefaultValue();
            }

            if (type.IsMultipleRelation(out _)
                && values.Count == 1)
            {
                return values.Single().Value;
            }

            if (values.Count == 1
                && values.Single().Value is string json
                && type != typeof(string)
                && !type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum
                && !type.IsAnonymous()
                && !type.IsDatabaseEntity()
                && !type.IsInlinedObject())
            {
                return _jsonSerializer.DeserializeObject(json, type);
            }

            return _objectBuilder.Build(type, values);
        }

        private IDictionary<string, object?> ArrangeValues(Type type, IDictionary<string, object?> values)
        {
            if (type.IsSubclassOfOpenGeneric(typeof(IUniqueIdentified<>))
                && values.Count == 1
                && values.Single().Value == null)
            {
                return values;
            }

            if (type.IsMultipleRelation(out _)
                && values.Count == 1)
            {
                return values;
            }

            if (values.Count == 1
                && values.Single().Value is string
                && type != typeof(string)
                && !type.ExtractGenericArgumentAtOrSelf(typeof(Nullable<>)).IsEnum
                && !type.IsAnonymous()
                && !type.IsDatabaseEntity()
                && !type.IsInlinedObject())
            {
                return values;
            }

            if (type.IsAnonymous())
            {
                values = values
                   .ToDictionary(
                        pair => pair.Key.Split("_", StringSplitOptions.RemoveEmptyEntries).Last(),
                        pair => pair.Value,
                        StringComparer.OrdinalIgnoreCase);
            }
            else if (!type.IsPrimitive())
            {
                values = values
                   .GroupBy(pair =>
                        {
                            var parts = pair.Key.Split("_", StringSplitOptions.RemoveEmptyEntries);
                            return parts.First();
                        },
                        pair =>
                        {
                            var parts = pair.Key.Split("_", StringSplitOptions.RemoveEmptyEntries);
                            return parts.Length > 1
                                ? new KeyValuePair<string, object?>(string.Join("_", parts.Skip(1)), pair.Value)
                                : pair;
                        })
                   .ToDictionary(grp => grp.Key,
                        grp =>
                        {
                            var column = type.Column(grp.Key);

                            if (column.Declared.IsJsonColumn())
                            {
                                return _jsonSerializer.DeserializeObject((string)grp.Single().Value!, column.PropertyType);
                            }

                            var innerValues = grp.ToDictionary(innerKey => innerKey.Key, innerValue => innerValue.Value);
                            return Build(column.PropertyType, innerValues);
                        },
                        StringComparer.OrdinalIgnoreCase);
            }

            return values;
        }

        private bool TryGetValue(
            IAdvancedDatabaseTransaction transaction,
            Type entity,
            object key,
            [NotNullWhen(true)] out object? stored)
        {
            stored = GetType()
               .CallMethod(nameof(TryGetValue))
               .WithTypeArguments(entity)
               .WithArguments(transaction.Store, key)
               .Invoke();

            return stored != default;
        }

        private static TEntity? TryGetValue<TEntity>(
            ITransactionalStore transactionalStore,
            object key)
            where TEntity : IUniqueIdentified
        {
            return transactionalStore.TryGetValue<TEntity>(key, out var stored)
                ? stored
                : default;
        }

        private static void Store(
            IAdvancedDatabaseTransaction transaction,
            IUniqueIdentified built)
        {
            transaction.Store.Store(built);
        }

        private IReadOnlyDictionary<ColumnInfo, object?> ReplaceRelations(
            Type type,
            IDictionary<string, object?> values)
        {
            return _modelProvider
                .Columns(type)
                .Where(column => column.IsRelation)
                .ToDictionary(
                    column => column,
                    column =>
                    {
                        var value = values[column.Name];

                        values.Remove(column.Name);
                        values[column.Relation.Property.Name] = column.Relation.Target.DefaultValue();

                        return value;
                    });
        }

        private static async Task MaterializeRelations(
            IAdvancedDatabaseTransaction transaction,
            object? built,
            IReadOnlyDictionary<ColumnInfo, object?> relationValues,
            CancellationToken token)
        {
            foreach (var (column, primaryKey) in relationValues)
            {
                if (primaryKey != null)
                {
                    var relation = await MaterializeRelation(transaction, column.Relation.Target, primaryKey, token).ConfigureAwait(false);
                    column.Relation.Property.Declared.SetValue(built, relation);
                }
            }
        }

        private static Task<object?> MaterializeRelation(
            IAdvancedDatabaseTransaction transaction,
            Type type,
            object primaryKey,
            CancellationToken token)
        {
            var keyType = type.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));

            var task = typeof(SqlCommandMaterializer)
                .CallMethod(nameof(MaterializeRelation))
                .WithTypeArguments(type, keyType)
                .WithArguments(transaction, primaryKey, token)
                .Invoke<Task>();

            return typeof(SqlCommandMaterializer)
                .CallMethod(nameof(AsEntity))
                .WithTypeArguments(type, keyType)
                .WithArgument(task)
                .Invoke<Task<object?>>();
        }

        private static Task<TEntity?> MaterializeRelation<TEntity, TKey>(
            IAdvancedDatabaseTransaction transaction,
            TKey primaryKey,
            CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            if (transaction.Store.TryGetValue<TEntity>(primaryKey, out var entity))
            {
                return Task.FromResult<TEntity?>(entity);
            }

            return transaction
                .All<TEntity>()
                .Where(databaseEntity => Equals(databaseEntity.PrimaryKey, primaryKey))
                .SingleOrDefaultAsync(token);
        }

        private static async Task<object?> AsEntity<TEntity, TKey>(Task<TEntity?> task)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            return await task.ConfigureAwait(false);
        }

        private IReadOnlyDictionary<ColumnInfo, object?> AddMultipleRelations(
            Type type,
            IDictionary<string, object?> values)
        {
            return _modelProvider
                .Columns(type)
                .Where(column => column.IsMultipleRelation)
                .ToDictionary(
                    column => column,
                    column =>
                    {
                        var value = column.Relation.Property.PropertyType.IsNullable()
                            ? column.Relation.Property.PropertyType.DefaultValue()
                            : Activator.CreateInstance(column.Relation.Target.ConstructMultipleRelationType());

                        values[column.Relation.Property.Name] = value;

                        return value;
                    });
        }

        private static Task MaterializeMultipleRelations(
            IAdvancedDatabaseTransaction transaction,
            object? built,
            Type type,
            IReadOnlyDictionary<ColumnInfo, object?> relationValues,
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
            IReadOnlyDictionary<ColumnInfo, object?> relationValues,
            CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            foreach (var (column, collection) in relationValues)
            {
                if (collection == null)
                {
                    continue;
                }

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
                .ToListAsync(token)
                .ConfigureAwait(false);

            items.Each(collection.Add);
        }
    }
}