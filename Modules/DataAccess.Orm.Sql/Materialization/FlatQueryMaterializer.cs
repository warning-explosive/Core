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

            var relations = _modelProvider
                .Columns(type)
                .Where(column => column.IsRelation)
                .ToArray();

            var relationValues = ReplaceRelations(values, relations);

            var built = (T)Build(type, values) !;

            if (type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                Store(type, built);
            }

            await MaterializeRelations(built, relationValues, token).ConfigureAwait(false);

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

        private static IReadOnlyDictionary<ColumnInfo, object?> ReplaceRelations(
            IDictionary<string, object?> values,
            ColumnInfo[] relations)
        {
            var relationValues = new Dictionary<ColumnInfo, object?>();

            foreach (var column in relations)
            {
                relationValues.Add(column, values[column.Name]);
                values.Remove(column.Name);

                values[column.Relation.Property.Name] = column.Relation.Target.DefaultValue();
            }

            return relationValues;
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
                    column.Relation.Property.SetValue(built, relation);
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
    }
}