namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Connection;
    using CrossCuttingConcerns.ObjectBuilder;
    using CrossCuttingConcerns.Settings;
    using Extensions;
    using Microsoft.Extensions.Logging;
    using Model;
    using Orm.Extensions;
    using Orm.Linq;
    using Orm.Settings;
    using Translation;
    using Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryMaterializer<T> : IQueryMaterializer<FlatQuery, T>,
                                              IResolvable<IQueryMaterializer<FlatQuery, T>>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IModelProvider _modelProvider;
        private readonly IObjectBuilder _objectBuilder;
        private readonly IDatabaseProvider _databaseProvider;
        private readonly ILogger _logger;

        public FlatQueryMaterializer(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction transaction,
            IModelProvider modelProvider,
            IObjectBuilder objectBuilder,
            IDatabaseProvider databaseProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _transaction = transaction;
            _modelProvider = modelProvider;
            _objectBuilder = objectBuilder;
            _databaseProvider = databaseProvider;
            _logger = logger;
        }

        public Task<T> MaterializeScalar(FlatQuery query, CancellationToken token)
        {
            var scalar = MaterializeInternal(query, token)
                .AsEnumerable(token)
                .SingleOrDefault();

            return Task.FromResult(scalar);
        }

        public IAsyncEnumerable<T> Materialize(FlatQuery query, CancellationToken token)
        {
            return MaterializeInternal(query, token);
        }

        private async IAsyncEnumerable<T> MaterializeInternal(FlatQuery query, [EnumeratorCancellation] CancellationToken token)
        {
            var dynamicResult = await GetDynamicResult(query, token).ConfigureAwait(false);

            foreach (var dynamicValues in dynamicResult)
            {
                token.ThrowIfCancellationRequested();

                var values = (dynamicValues as IDictionary<string, object?>) !;

                var built = await MaterializeInternal(values, token).ConfigureAwait(false);

                yield return built;
            }
        }

        private async Task<IEnumerable<dynamic>?> GetDynamicResult(FlatQuery query, CancellationToken token)
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var commandText = InlineQueryParameters(_dependencyContainer, query);

            return await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), _transaction.Invoke)
               .Catch<Exception>()
               .Invoke(_databaseProvider.Handle<IEnumerable<dynamic>>(commandText), token)
               .ConfigureAwait(false);

            static string InlineQueryParameters(
                IDependencyContainer dependencyContainer,
                FlatQuery query)
            {
                var sqlQuery = query.Query;

                foreach (var (name, value) in query.QueryParameters)
                {
                    if (!query.Query.Contains($"@{name}", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    sqlQuery = sqlQuery.Replace($"@{name}", value.QueryParameterSqlExpression(dependencyContainer), StringComparison.OrdinalIgnoreCase);
                }

                return sqlQuery;
            }
        }

        private async Task<T> MaterializeInternal(
            IDictionary<string, object?> values,
            CancellationToken token)
        {
            var type = typeof(T);

            var relationValues = ReplaceRelations(type, values);

            var multipleRelationValues = AddMultipleRelations(type, values);

            T built;

            if (!type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                built = (T)Build(type, values) !;
            }
            else
            {
                if (TryGetValue(type, values[nameof(IUniqueIdentified.PrimaryKey)] !, out var stored))
                {
                    Fill(type, stored, values);
                    built = (T)stored;
                }
                else
                {
                    built = (T)Build(type, values) !;
                    Store(type, built);
                }
            }

            await MaterializeRelations(built, relationValues, token).ConfigureAwait(false);

            await MaterializeMultipleRelations(built, multipleRelationValues, token).ConfigureAwait(false);

            return built;
        }

        private void Fill(Type type, object instance, IDictionary<string, object?> rawValues)
        {
            _objectBuilder.Fill(type, instance, ArrangeValues(type, rawValues));
        }

        private object? Build(Type type, IDictionary<string, object?> rawValues)
        {
            var values = ArrangeValues(type, rawValues);

            if (type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>))
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

            return _objectBuilder.Build(type, values);
        }

        private IDictionary<string, object?> ArrangeValues(Type type, IDictionary<string, object?> values)
        {
            if (type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>))
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
                            var innerType = type.Column(grp.Key).PropertyType;
                            var innerValues = grp.ToDictionary(innerKey => innerKey.Key, innerValue => innerValue.Value);
                            return Build(innerType, innerValues);
                        },
                        StringComparer.OrdinalIgnoreCase);
            }

            return values;
        }

        private bool TryGetValue(Type entity, object key, [NotNullWhen(true)] out object? stored)
        {
            stored = GetType()
               .CallMethod(nameof(TryGetValue))
               .WithTypeArguments(entity, entity.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>)))
               .WithArguments(_transaction.Store, key)
               .Invoke();

            return stored != default;
        }

        private static TEntity? TryGetValue<TEntity, TKey>(
            ITransactionalStore transactionalStore,
            TKey key)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            return transactionalStore.TryGetValue<TEntity, TKey>(key, out var stored)
                ? stored
                : default;
        }

        private void Store(Type entity, object built)
        {
            _ = GetType()
               .CallMethod(nameof(Store))
               .WithTypeArguments(entity, entity.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>)))
               .WithArguments(_transaction.Store, built)
               .Invoke();
        }

        private static void Store<TEntity, TKey>(
            ITransactionalStore transactionalStore,
            TEntity entity)
            where TEntity : IUniqueIdentified<TKey>
            where TKey : notnull
        {
            transactionalStore.Store<TEntity, TKey>(entity);
        }

        private IReadOnlyDictionary<ColumnInfo, object?> ReplaceRelations(
            Type type,
            IDictionary <string, object?> values)
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

        private async Task MaterializeRelations(
            T built,
            IReadOnlyDictionary<ColumnInfo, object?> relationValues,
            CancellationToken token)
        {
            foreach (var (column, primaryKey) in relationValues)
            {
                if (primaryKey != null)
                {
                    var relation = await MaterializeRelation(column.Relation.Target, primaryKey, token).ConfigureAwait(false);
                    column.Relation.Property.Declared.SetValue(built, relation);
                }
            }
        }

        private Task<object?> MaterializeRelation(
            Type type,
            object primaryKey,
            CancellationToken token)
        {
            var keyType = type.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));

            var task = this
                .CallMethod(nameof(MaterializeRelation))
                .WithTypeArguments(type, keyType)
                .WithArguments(primaryKey, token)
                .Invoke<Task>();

            return GetType()
                .CallMethod(nameof(AsEntity))
                .WithTypeArguments(type, keyType)
                .WithArgument(task)
                .Invoke<Task<object?>>();
        }

        private Task<TEntity?> MaterializeRelation<TEntity, TKey>(
            TKey primaryKey,
            CancellationToken token)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            if (_transaction.Store.TryGetValue<TEntity, TKey>(primaryKey, out var entity))
            {
                return Task.FromResult<TEntity?>(entity);
            }

            return _transaction
                .Read<TEntity, TKey>()
                .All()
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

        private Task MaterializeMultipleRelations(
            T built,
            IReadOnlyDictionary<ColumnInfo, object?> relationValues,
            CancellationToken token)
        {
            if (!relationValues.Any())
            {
                return Task.CompletedTask;
            }

            if (!typeof(T).IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                throw new InvalidOperationException($"Projection {typeof(T).FullName} should implement {nameof(IDatabaseEntity<object>)} so as to have multiple relation fields");
            }

            var type = typeof(T);
            var keyType = type.ExtractGenericArgumentAt(typeof(IUniqueIdentified<>));

            return this
                .CallMethod(nameof(MaterializeMultipleRelations))
                .WithTypeArguments(type, keyType)
                .WithArguments(built!, relationValues, token)
                .Invoke<Task>();
        }

        private async Task MaterializeMultipleRelations<TEntity, TKey>(
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

                await this
                    .CallMethod(nameof(MaterializeMultipleRelation))
                    .WithTypeArguments(mtmType, ownerType, collectionItemType, leftKeyType, rightKeyType)
                    .WithArguments(built.PrimaryKey, collection, token)
                    .Invoke<Task>()
                    .ConfigureAwait(false);
            }
        }

        private async Task MaterializeMultipleRelation<TMtm, TLeft, TRight, TLeftKey, TRightKey>(
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

            var subQuery = _transaction
                .Read<TMtm, object>()
                .All()
                .Where(mtm => Equals(mtm.Left, ownerPrimaryKey))
                .Select(mtm => mtm.Right);

            var items = await _transaction
                .Read<TRight, TRightKey>()
                .All()
                .Where(databaseEntity => subQuery.Contains(databaseEntity.PrimaryKey))
                .ToListAsync(token)
                .ConfigureAwait(false);

            items.Each(collection.Add);
        }
    }
}