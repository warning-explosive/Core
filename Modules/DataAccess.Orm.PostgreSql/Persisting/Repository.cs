namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Extensions;
    using Linq;
    using Microsoft.Extensions.Logging;
    using Orm.Connection;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Extensions;
    using Transaction;

    [SuppressMessage("Analysis", "CA1506", Justification = "Infrastructural code")]
    [Component(EnLifestyle.Singleton)]
    internal class Repository : IRepository,
                                IResolvable<IRepository>
    {
        private const string OnConflictDoNothing = @" on conflict do nothing";
        private const string OnConflictDoUpdate = @" on conflict ({0}) do update set {1}";
        private const string SetExpressionFormat = @"{0} = {1}";
        private const string ValuesFormat = @"({0})";
        private const string ColumnFormat = @"""{0}""";
        private const string ExcludedPseudoColumnFormat = @"excluded.{0}";

        private const string InsertQueryFormat = @"insert into ""{0}"".""{1}""({2}) values {3}{4}";

        private const string UpdateValueQueryFormat = @"update ""{0}"".""{1}"" a set {2} where {3}";

        private const string DeleteValueQueryFormat = @"delete from ""{0}"".""{1}"" a where {2}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IQueryTranslator _translator;
        private readonly IDatabaseImplementation _databaseImplementation;
        private readonly ILogger _logger;

        public Repository(
            IDependencyContainer dependencyContainer,
            ISettingsProvider<OrmSettings> settingsProvider,
            IModelProvider modelProvider,
            IQueryTranslator translator,
            IDatabaseImplementation databaseImplementation,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _translator = translator;
            _databaseImplementation = databaseImplementation;
            _logger = logger;
        }

        public async Task<long> Insert(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<IDatabaseEntity> entities,
            EnInsertBehavior insertBehavior,
            CancellationToken token)
        {
            if (!entities.Any())
            {
                return default;
            }

            var settings = await _settingsProvider
               .Get(token)
               .ConfigureAwait(false);

            IReadOnlyDictionary<object, IUniqueIdentified> map = entities
               .SelectMany(entity => entity.Flatten(_modelProvider))
               .DistinctBy(GetKey)
               .ToDictionary(GetKey);

            var version = await transaction
               .GetXid(settings, _logger, token)
               .ConfigureAwait(false);

            foreach (var entity in map.Values.OfType<IDatabaseEntity>().Where(entity => entity.Version == default))
            {
                entity.Version = version;
            }

            var commandText = map
               .Values
               .OrderByDependencies(GetKey, GetDependencies(_modelProvider, map))
               .Stack(entity => entity.GetType())
               .Select(grp => InsertEntity(grp.Key, grp.Value, _dependencyContainer, _modelProvider, insertBehavior))
               .ToString(";" + Environment.NewLine);

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseImplementation.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            var change = new CreateEntityChange(entities, insertBehavior);

            transaction.CollectChange(change);

            return affectedRowsCount;

            static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> GetDependencies(
                IModelProvider modelProvider,
                IReadOnlyDictionary<object, IUniqueIdentified> map)
            {
                return entity => modelProvider
                   .Tables[entity.GetType()]
                   .Columns
                   .Values
                   .Where(column => column.IsRelation)
                   .Select(DependencySelector(entity, map))
                   .Where(dependency => dependency != null)
                   .Select(dependency => dependency!);

                static Func<ColumnInfo, IUniqueIdentified?> DependencySelector(
                    IUniqueIdentified entity,
                    IReadOnlyDictionary<object, IUniqueIdentified> map)
                {
                    return column => entity.GetType().IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>))
                        ? map[GetKey(column.Relation.Target, column.GetValue(entity) !)]
                        : column.GetRelationValue(entity);
                }
            }

            static string InsertEntity(
                Type type,
                IEnumerable<IUniqueIdentified> entities,
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

                return InsertQueryFormat.Format(
                    table.Schema,
                    table.Name,
                    columns.ToString(", "),
                    values,
                    ApplyInsertBehavior(table, insertBehavior, columns));
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
                       .Where(column => !column.Equals(nameof(IUniqueIdentified.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                       .ToArray();

                    return !columns.Any() || table.Type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>))
                        ? OnConflictDoNothing
                        : OnConflictDoUpdate.Format(KeyColumns(table), Update(columns));

                    static string KeyColumns(ITableInfo table)
                    {
                        var uniqueIndexColumns = table.Indexes.Values.SingleOrDefault(index => index.Unique)?.Columns.Select(column => column.Name)
                                              ?? new[] { nameof(IDatabaseEntity.PrimaryKey) };

                        return string.Join(", ", uniqueIndexColumns.Select(column => ColumnFormat.Format(column)));
                    }

                    static string Update(IEnumerable<string> columns)
                    {
                        return columns
                           .Select(column => SetExpressionFormat.Format(column, ExcludedPseudoColumnFormat.Format(column)))
                           .ToString("," + Environment.NewLine);
                    }
                }
            }
        }

        public Task<long> Update<TEntity, TValue>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, TValue>> accessor,
            Expression<Func<TEntity, TValue>> valueProducer,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            return Update(transaction, new[] { new UpdateInfo<TEntity>(Lift(accessor), Lift(valueProducer)) }, predicate, token);

            static Expression<Func<TEntity, object?>> Lift(Expression<Func<TEntity, TValue>> expression)
            {
                return Expression.Lambda<Func<TEntity, object?>>(Expression.Convert(expression.Body, typeof(object)), expression.Parameters);
            }
        }

        public async Task<long> Update<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<UpdateInfo<TEntity>> infos,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var setExpressions = new List<string>(infos.Count);

            foreach (var info in infos)
            {
                var chain = info
                   .Accessor
                   .ExtractMembersChain()
                   .Select(property => new ColumnProperty(property, property))
                   .ToArray();

                var column = new ColumnInfo(
                    table,
                    chain,
                    _modelProvider);

                if (column.IsMultipleRelation)
                {
                    throw new NotSupportedException($"Unable to update multiple relation: {column.Name}");
                }

                var columnExpression = @$"""{column.Name}""";

                var valueQuery = (FlatQuery)_translator.Translate(info.ValueProducer);

                var valueExpression = InlineQueryParameters(valueQuery.CommandText, valueQuery.CommandParameters);

                setExpressions.Add(SetExpressionFormat.Format(columnExpression, valueExpression));
            }

            var predicateSqlQuery = (FlatQuery)_translator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(predicateSqlQuery.CommandText, predicateSqlQuery.CommandParameters);

            var commandText = UpdateValueQueryFormat.Format(
                table.Schema,
                table.Name,
                string.Join(", ", setExpressions),
                predicateExpression);

            // TODO: #209 - recode query
            var versions = (await transaction
                   .All<TEntity>()
                   .Where(predicate)
                   .Select(entity => entity.Version)
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var updateVersion = await transaction
               .GetXid(settings, _logger, token)
               .ConfigureAwait(false);

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseImplementation.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                var change = new UpdateEntityChange<TEntity>(
                    version,
                    count,
                    infos,
                    predicate,
                    updateVersion);

                transaction.CollectChange(change);
            }

            return affectedRowsCount;
        }

        public async Task<long> Delete<TEntity>(
            IAdvancedDatabaseTransaction transaction,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken token)
            where TEntity : IDatabaseEntity
        {
            // TODO: #178 - add delete behaviors
            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var predicateSqlQuery = (FlatQuery)_translator.Translate(predicate);

            var predicateExpression = InlineQueryParameters(predicateSqlQuery.CommandText, predicateSqlQuery.CommandParameters);

            var commandText = DeleteValueQueryFormat.Format(
                table.Schema,
                table.Name,
                predicateExpression);

            // TODO: #209 - recode query
            var versions = (await transaction
                   .All<TEntity>()
                   .Where(predicate)
                   .Select(entity => entity.Version)
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings, _logger), transaction.Execute)
               .Catch<Exception>()
               .Invoke(_databaseImplementation.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            foreach (var (version, count) in versions)
            {
                var change = new DeleteEntityChange<TEntity>(
                    version,
                    count,
                    predicate);

                transaction.CollectChange(change);
            }

            return affectedRowsCount;
        }

        private static string InlineQueryParameters(
            string query,
            IReadOnlyDictionary<string, string> queryParameters)
        {
            // TODO use ADO.NET and NpgsqlParameter
            foreach (var (name, value) in queryParameters)
            {
                if (!query.Contains($"@{name}", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                query = query.Replace($"@{name}", value, StringComparison.OrdinalIgnoreCase);
            }

            return query;
        }

        private static object GetKey(IUniqueIdentified entity)
        {
            return new
            {
                Type = entity.GetType(),
                entity.PrimaryKey
            };
        }

        private static object GetKey(Type type, object primaryKey)
        {
            return new
            {
                Type = type,
                PrimaryKey = primaryKey
            };
        }
    }
}