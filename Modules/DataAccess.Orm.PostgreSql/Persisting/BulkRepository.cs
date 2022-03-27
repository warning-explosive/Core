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
        private const string InsertQueryFormat = @"insert into ""{0}"".""{1}""({2}) values {3}{4}";
        private const string OnConflictDoNothing = @" on conflict do nothing";
        private const string OnConflictDoUpdate = @" on conflict (""PrimaryKey"") do update set {0}";
        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" set {2} where ""PrimaryKey"" in {3}";
        private const string SetExpressionFormat = @"{0} = {1}";
        private const string DeleteValueQueryFormat = @"delete ""{0}"".""{1}"" where ""PrimaryKey"" in {2}";
        private const string ValuesFormat = @"({0})";
        private const string ColumnFormat = @"""{0}""";
        private const string ExcludedPseudoColumnFormat = @"excluded.{0}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IDatabaseTransaction _transaction;
        private readonly IExpressionTranslator _expressionTranslator;

        public BulkRepository(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IDatabaseTransaction transaction,
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
            EnInsertBehavior insertBehavior,
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
                .SelectMany(grp => InsertEntity(grp.Key, grp.Value, _dependencyContainer, _modelProvider, insertBehavior)
                    .Concat(InsertMtm(grp.Key, grp.Value, _dependencyContainer, _modelProvider, insertBehavior)))
                .ToString(";" + Environment.NewLine);

            try
            {
                _ = await _transaction
                   .UnderlyingDbTransaction
                   .InvokeScalar(commandText, settings, token)
                   .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }

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
                    var table = modelProvider.Tables[type];

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
                IModelProvider modelProvider,
                EnInsertBehavior insertBehavior)
            {
                var table = modelProvider.Tables[type];

                var flatColumns = table
                   .Columns
                   .Values
                   .Where(column => !column.IsMultipleRelation)
                   .ToArray();

                var columns = flatColumns
                   .Select(column => ColumnFormat.Format(column.Name))
                   .ToArray();

                var values = entities
                   .Select(entity => ValuesFormat
                       .Format(flatColumns
                           .Select(column => column
                               .GetValue(entity)
                               .QueryParameterSqlExpression(dependencyContainer))
                           .ToString(", ")))
                   .ToString("," + Environment.NewLine);

                yield return InsertQueryFormat.Format(
                    table.Schema,
                    table.Name,
                    columns.ToString(", "),
                    values,
                    ApplyInsertBehavior(table, insertBehavior, columns));
            }

            static IEnumerable<string> InsertMtm(
                Type type,
                IEnumerable<IUniqueIdentified<TKey>> entities,
                IDependencyContainer dependencyContainer,
                IModelProvider modelProvider,
                EnInsertBehavior insertBehavior)
            {
                var table = modelProvider.Tables[type];

                var mtmColumns = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation);

                foreach (var column in mtmColumns)
                {
                    var mtmTable = modelProvider.MtmTables[column.MultipleRelationTable!];

                    var columns = (type == mtmTable.Left
                            ? new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right) }
                            : new[] { nameof(BaseMtmDatabaseEntity<Guid, Guid>.Right), nameof(BaseMtmDatabaseEntity<Guid, Guid>.Left) })
                        .Select(column => ColumnFormat.Format(column))
                        .ToArray();

                    var values = entities
                       .SelectMany(entity => column
                           .GetMultipleRelationValue(entity)
                           .Select(foreignKey => ValuesFormat.Format(new object[]
                            {
                                entity.PrimaryKey.QueryParameterSqlExpression(dependencyContainer),
                                foreignKey.PrimaryKey.QueryParameterSqlExpression(dependencyContainer)
                            }.ToString(", "))))
                       .ToString("," + Environment.NewLine);

                    yield return InsertQueryFormat.Format(
                        mtmTable.Schema,
                        mtmTable.Name,
                        columns.ToString(", "),
                        values,
                        ApplyInsertBehavior(mtmTable, insertBehavior, columns));
                }
            }

            static string ApplyInsertBehavior(
                ITableInfo table,
                EnInsertBehavior insertBehavior,
                string[] columns)
            {
                return insertBehavior switch
                {
                    EnInsertBehavior.Default => string.Empty,
                    EnInsertBehavior.DoNothing => OnConflictDoNothing,
                    EnInsertBehavior.DoUpdate => ApplyUpdateInsertBehavior(table, columns),
                    _ => throw new NotSupportedException(insertBehavior.ToString())
                };

                static string ApplyUpdateInsertBehavior(ITableInfo table, string[] columns)
                {
                    columns = columns
                       .Where(column => !column.Equals(nameof(IDatabaseEntity<Guid>.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                       .ToArray();

                    return !columns.Any() || table.Type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>))
                        ? OnConflictDoNothing
                        : OnConflictDoUpdate.Format(Update(columns));

                    static string Update(IEnumerable<string> columns)
                    {
                        return columns
                           .Select(column => SetExpressionFormat.Format(column, ExcludedPseudoColumnFormat.Format(column)))
                           .ToString("," + Environment.NewLine);
                    }
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
            var table = _modelProvider.Tables[type];

            var visitor = new ExtractMemberChainExpressionVisitor();
            _ = visitor.Visit(accessor);

            var column = new ColumnInfo(
                table,
                visitor.Chain.Select(property => new ColumnProperty(property, property)).ToArray(),
                _modelProvider);

            if (column.IsMultipleRelation)
            {
                throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
            }

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                SetExpressionFormat.Format(ColumnFormat.Format(column.Name), value.QueryParameterSqlExpression(_dependencyContainer)),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .InvokeScalar(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
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
            var table = _modelProvider.Tables[type];

            var visitor = new ExtractMemberChainExpressionVisitor();
            _ = visitor.Visit(accessor);

            var column = new ColumnInfo(
                table,
                visitor.Chain.Select(property => new ColumnProperty(property, property)).ToArray(),
                _modelProvider);

            if (column.IsMultipleRelation)
            {
                throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
            }

            var valueExpression = _expressionTranslator
                .Translate(valueProducer)
                .Translate(_dependencyContainer, 0);

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                SetExpressionFormat.Format(ColumnFormat.Format(column.Name), valueExpression),
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .Invoke(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
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
            var table = _modelProvider.Tables[type];

            var commandText = DeleteValueQueryFormat.Format(
                table.Type,
                table.Name,
                primaryKeys.QueryParameterSqlExpression(_dependencyContainer));

            try
            {
                _ = await _transaction
                    .UnderlyingDbTransaction
                    .Invoke(commandText, settings, token)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(commandText, exception);
            }
        }
    }
}