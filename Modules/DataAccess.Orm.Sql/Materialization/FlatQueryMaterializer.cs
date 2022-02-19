namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Extensions;
    using Model;
    using Orm.Linq;
    using Orm.Model;
    using Orm.Settings;
    using Translation;
    using Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryMaterializer<T> : IQueryMaterializer<FlatQuery, T>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IModelProvider _modelProvider;
        private readonly IObjectBuilder _objectBuilder;

        public FlatQueryMaterializer(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IAdvancedDatabaseTransaction transaction,
            IModelProvider modelProvider,
            IObjectBuilder objectBuilder)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _transaction = transaction;
            _modelProvider = modelProvider;
            _objectBuilder = objectBuilder;
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

            return await _transaction
                .UnderlyingDbTransaction
                .Invoke(commandText, settings, token)
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

            var built = (T)Build(type, values) !;

            if (type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                Store(type, built);
            }

            await MaterializeRelations(built, relationValues, token).ConfigureAwait(false);

            await MaterializeMultipleRelations(built, multipleRelationValues, token).ConfigureAwait(false);

            return built;
        }

        private object? Build(Type type, IDictionary<string, object?> values)
        {
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

            if (!type.IsPrimitive())
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
                    .ToDictionary(
                        grp => grp.Key,
                        grp =>
                        {
                            var innerType = type.Column(grp.Key).PropertyType;
                            var innerValues = grp.ToDictionary(innerKey => innerKey.Key, innerValue => innerValue.Value);
                            return Build(innerType, innerValues);
                        });
            }

            return _objectBuilder.Build(type, values);
        }

        private void Store(Type entity, T built)
        {
            _ = _transaction
                .CallMethod(nameof(_transaction.Store))
                .WithTypeArguments(entity, entity.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single())
                .WithArgument(built)
                .Invoke();
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
            var keyType = type.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single();

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
            if (_transaction.TryGetValue<TEntity, TKey>(primaryKey, out var entity))
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
            var keyType = type.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single();

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
                var typeArguments = mtmType.ExtractGenericArguments(typeof(BaseMtmDatabaseEntity<,>)).Single();
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
            where TMtm : BaseMtmDatabaseEntity<TLeftKey, TRightKey>, IUniqueIdentified<object>
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