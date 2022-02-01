namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
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

                var type = typeof(T);

                var values = (dynamicValues as IDictionary<string, object?>) !;

                var built = await MaterializeInternal(query, type, values, token).ConfigureAwait(false);

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
            FlatQuery query,
            Type type,
            IDictionary<string, object?> values,
            CancellationToken token)
        {
            var relations = _modelProvider
                .Columns(type)
                .Where(column => column.IsRelation)
                .ToArray();

            var mtmRelations = _modelProvider
                .Columns(type)
                .Where(column => column.IsMultipleRelation)
                .ToArray();

            if (relations.Any())
            {
                MockRelations(type, values, relations);
            }

            if (mtmRelations.Any())
            {
                MockMtmRelations(type, values, mtmRelations);
            }

            var built = (T)_objectBuilder.Build(type, values) !;

            if (type.IsSubclassOfOpenGeneric(typeof(IDatabaseEntity<>)))
            {
                Store(type, built);
            }

            if (relations.Any())
            {
                await MaterializeRelations(query, type, values, relations).ConfigureAwait(false);
            }

            if (mtmRelations.Any())
            {
                await MaterializeMtmRelations(query, type, values, mtmRelations).ConfigureAwait(false);
            }

            return built;
        }

        private void Store(Type entity, object built)
        {
            _ = _transaction
                .CallMethod(nameof(_transaction.Store))
                .WithTypeArguments(entity, entity.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single())
                .WithArguments(built)
                .Invoke();
        }

        private static void MockRelations(
            Type type,
            IDictionary<string, object?> values,
            ColumnInfo[] relationColumns)
        {
            foreach (var column in relationColumns)
            {
                values.Remove(column.Name);
                values[column.Relation.Property.Name] = column.Relation.Target.DefaultValue();
            }
        }

        private Task MaterializeRelations(
            FlatQuery query,
            Type type,
            IDictionary<string, object?> values,
            ColumnInfo[] relationColumns)
        {
            throw new NotImplementedException();
        }

        private static void MockMtmRelations(
            Type type,
            IDictionary<string, object?> values,
            ColumnInfo[] mtmRelationColumns)
        {
            foreach (var column in mtmRelationColumns)
            {
                values.Remove(column.Name);
                values[column.Relation.Property.Name] = Activator.CreateInstance(column.Type);
            }
        }

        private Task MaterializeMtmRelations(
            FlatQuery query,
            Type type,
            IDictionary<string, object?> values,
            ColumnInfo[] mtmRelationColumns)
        {
            throw new NotImplementedException();
        }

        private object? MaterializeRelation(
            FlatQuery query,
            Type type,
            object primaryKey)
        {
            return this
                .CallMethod(nameof(MaterializeRelation))
                .WithTypeArguments(type, type.ExtractGenericArgumentsAt(typeof(IDatabaseEntity<>)).Single())
                .WithArguments(query, primaryKey)
                .Invoke<object?>();
        }

        private TEntity? MaterializeRelation<TEntity, TKey>(
            FlatQuery query,
            TKey primaryKey)
            where TEntity : IDatabaseEntity<TKey>
            where TKey : notnull
        {
            if (_transaction.TryGetValue<TEntity, TKey>(primaryKey, out var entity))
            {
                return entity;
            }

            // TODO: run sub-query
            throw new NotImplementedException();
        }
    }
}