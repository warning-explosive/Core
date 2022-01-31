namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using CrossCuttingConcerns.Api.Abstractions;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Extensions;

    [Component(EnLifestyle.Scoped)]
    internal class BulkRepository<TEntity, TKey> : IBulkRepository<TEntity, TKey>
        where TEntity : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        private const string InsertQueryFormat = @"insert into ""{0}"".""{1}""({2}) values {3}";
        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" set ""{2}"" = {3} where ""PrimaryKey"" in {4}";
        private const string DeleteValueQueryFormat = @"delete ""{0}"".""{1}"" where ""PrimaryKey"" in {2}";
        private const string ValuesFormat = "({0})";
        private const string ColumnFormat = @"""{0}""";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IExpressionTranslator _expressionTranslator;

        public BulkRepository(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IAdvancedDatabaseTransaction transaction,
            IExpressionTranslator expressionTranslator)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _transaction = transaction;
            _expressionTranslator = expressionTranslator;
        }

        public async Task Insert(
            TEntity[] entities,
            CancellationToken token)
        {
            if (!entities.Any())
            {
                return;
            }

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var commandText = entities
                .SelectMany(entity => Flatten(entity, _modelProvider, new List<IUniqueIdentified<TKey>>()))
                .OrderByDependencies(GetKey, GetDependencies(_modelProvider))
                .Stack(GetKey)
                .SelectMany(grp => InsertEntity(grp.Key, grp.Value, _dependencyContainer, _modelProvider)
                    .Concat(InsertMtm(grp.Key, grp.Value, _dependencyContainer, _modelProvider)))
                .ToString(";" + Environment.NewLine);

            _ = await _transaction
                .UnderlyingDbTransaction
                .InvokeScalar(commandText, settings, token)
                .ConfigureAwait(false);

            static IEnumerable<IUniqueIdentified<TKey>> Flatten(
                IUniqueIdentified<TKey> entity,
                IModelProvider modelProvider,
                ICollection<IUniqueIdentified<TKey>> visited)
            {
                visited.Add(entity);

                return GetDependencies(modelProvider)(entity)
                    .Where(dependency => !visited.Contains(dependency))
                    .SelectMany(dependency => Flatten(dependency, modelProvider, visited))
                    .Concat(new[] { entity });
            }

            static Type GetKey(IUniqueIdentified<TKey> entity)
            {
                return entity.GetType();
            }

            static Func<IUniqueIdentified<TKey>, IEnumerable<IUniqueIdentified<TKey>>> GetDependencies(
                IModelProvider modelProvider)
            {
                return entity =>
                {
                    var type = entity.GetType();
                    var table = modelProvider.Objects[type.SchemaName()][type.TableName()];

                    return table
                        .Columns
                        .Values
                        .Where(column => column.IsRelation)
                        .Select(column => column.GetRelationValue(entity))
                        .Concat(table
                            .Columns
                            .Values
                            .Where(column => column.IsMultipleRelation)
                            .SelectMany(column => column.GetMultipleRelationValue(entity)))
                        .Where(dependency => dependency != null)
                        .Select(dependency => dependency!);
                };
            }

            static IEnumerable<string> InsertEntity(
                Type type,
                IEnumerable<IUniqueIdentified<TKey>> entities,
                IDependencyContainer dependencyContainer,
                IModelProvider modelProvider)
            {
                var table = modelProvider.Objects[type.SchemaName()][type.TableName()];

                var columns = table
                    .Columns
                    .Values
                    .Where(column => !column.IsMultipleRelation)
                    .Select(column => ColumnFormat.Format(column.Name))
                    .ToString(", ");

                var values = entities
                    .Select(entity => ValuesFormat
                        .Format(table
                            .Columns
                            .Values
                            .Where(column => !column.IsMultipleRelation)
                            .Select(column => column
                                .GetValue(entity)
                                .QueryParameterSqlExpression(dependencyContainer))
                            .ToString(", ")))
                    .ToString("," + Environment.NewLine);

                yield return InsertQueryFormat.Format(
                    table.Schema,
                    table.Type.TableName(),
                    columns,
                    values);
            }

            static IEnumerable<string> InsertMtm(
                Type type,
                IEnumerable<IUniqueIdentified<TKey>> entities,
                IDependencyContainer dependencyContainer,
                IModelProvider modelProvider)
            {
                var table = modelProvider.Objects[type.SchemaName()][type.TableName()];

                var mtmValueProducers = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .ToDictionary(
                        column => column,
                        column =>
                        {
                            var schema = DatabaseModelExtensions.MtmSchemaName(column.Relation.Source, column.Relation.Target);
                            return modelProvider.Objects[schema][column.MultipleRelationTable!.TableName()];
                        });

                foreach (var (column, mtmTable) in mtmValueProducers)
                {
                    var (left, _) = modelProvider.MtmTables[mtmTable.Schema][mtmTable.Type];

                    var columns = (type == left
                            ? new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }
                            : new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left) })
                        .Select(column => ColumnFormat.Format(column))
                        .ToString(", ");

                    var values = entities
                        .SelectMany(entity => column
                            .GetMultipleRelationValue(entity)
                            .Select(foreignKey => ValuesFormat.Format(
                                new object[]
                                    {
                                        entity.PrimaryKey.QueryParameterSqlExpression(dependencyContainer),
                                        foreignKey.PrimaryKey.QueryParameterSqlExpression(dependencyContainer)
                                    }.ToString(", "))))
                        .ToString("," + Environment.NewLine);

                    yield return InsertQueryFormat.Format(
                        mtmTable.Schema,
                        mtmTable.Type.TableName(),
                        columns,
                        values);
                }
            }
        }

        public async Task Update<TValue>(
            TKey[] primaryKeys,
            Expression<Func<TEntity, TValue>> accessor,
            TValue value,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Objects[type.SchemaName()][type.TableName()];

            var visitor = new ExtractMemberChainExpressionVisitor();
            _ = visitor.Visit(accessor);
            var column = new ColumnInfo(table.Schema, table.Type, visitor.Chain, _modelProvider);

            if (column.IsMultipleRelation)
            {
                throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
            }

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Type.TableName(),
                column.Name,
                value.QueryParameterSqlExpression(_dependencyContainer),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            _ = await _transaction
                .UnderlyingDbTransaction
                .InvokeScalar(commandText, settings, token)
                .ConfigureAwait(false);
        }

        public async Task Update<TValue>(
            TKey[] primaryKeys,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Objects[type.SchemaName()][type.TableName()];

            var visitor = new ExtractMemberChainExpressionVisitor();
            _ = visitor.Visit(accessor);
            var column = new ColumnInfo(table.Schema, table.Type, visitor.Chain, _modelProvider);

            if (column.IsMultipleRelation)
            {
                throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
            }

            var valueExpression = _expressionTranslator
                .Translate(valueProducer)
                .Translate(_dependencyContainer, 0);

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Type.TableName(),
                column.Name,
                valueExpression,
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            _ = await _transaction
                .UnderlyingDbTransaction
                .Invoke(commandText, settings, token)
                .ConfigureAwait(false);
        }

        public async Task Delete(
            TKey[] primaryKeys,
            CancellationToken token)
        {
            if (!primaryKeys.Any())
            {
                return;
            }

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Objects[type.SchemaName()][type.TableName()];

            var commandText = DeleteValueQueryFormat.Format(
                table.Schema,
                table.Type.TableName(),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            _ = await _transaction
                .UnderlyingDbTransaction
                .Invoke(commandText, settings, token)
                .ConfigureAwait(false);
        }
    }
}