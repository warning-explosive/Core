namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Connection
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using Basics;
    using Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Connection;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using Transaction;

    [SuppressMessage("Analysis", "CA1506", Justification = "Infrastructural code")]
    [Component(EnLifestyle.Singleton)]
    internal class Repository : IRepository,
                                IResolvable<IRepository>
    {
        private const string TransactionIdCommandText = "select txid_current()";

        private const string ColumnFormat = @"""{0}""";

        private const string InsertCommandFormat = @"insert into ""{0}"".""{1}""({2}) values {3}{4}";
        private const string ValuesFormat = @"({0})";
        private const string AssignExpressionFormat = @"{0} = {1}";
        private const string ExcludedPseudoColumnFormat = @"excluded.{0}";
        private const string OnConflictDoNothing = @" on conflict do nothing";
        private const string OnConflictDoUpdate = @" on conflict ({0}) do update set {1}";

        private const string UpdateCommandFormat = @"update ""{0}"".""{1}"" a set {2}";

        private const string DeleteCommandFormat = @"delete from ""{0}"".""{1}"" a where {2}";

        private readonly IModelProvider _modelProvider;
        private readonly IExpressionTranslator _translator;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        private readonly ConcurrentDictionary<InsertSqlExpressionCacheKey, TranslatedSqlExpression> _cache;

        public Repository(
            IModelProvider modelProvider,
            IExpressionTranslator translator,
            IDatabaseConnectionProvider connectionProvider)
        {
            _modelProvider = modelProvider;
            _translator = translator;
            _connectionProvider = connectionProvider;

            _cache = new ConcurrentDictionary<InsertSqlExpressionCacheKey, TranslatedSqlExpression>();
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

            IReadOnlyDictionary<EntityKey, IUniqueIdentified> map = entities
                .SelectMany(entity => Flatten(entity, _modelProvider))
                .DistinctBy(pair => pair.Key)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var version = await GetXid(_connectionProvider, transaction, token).ConfigureAwait(false);

            foreach (var entity in map.Values.OfType<IDatabaseEntity>().Where(entity => entity.Version == default))
            {
                entity.Version = version;
            }

            var commands = map
               .Values
               .OrderByDependencies(entity => new EntityKey(entity), GetDependencies(_modelProvider, map))
               .Stack(entity => entity.GetType())
               .SelectMany(grp => InsertEntity(_cache, _modelProvider.Tables[grp.Key], grp.Value, insertBehavior));

            var affectedRowsCount = await _connectionProvider
                .Execute(transaction, commands, token)
                .ConfigureAwait(false);

            var change = new CreateEntityChange(entities, insertBehavior);

            transaction.CollectChange(change);

            return affectedRowsCount;

            static Dictionary<EntityKey, IUniqueIdentified> Flatten(
                IUniqueIdentified source,
                IModelProvider modelProvider)
            {
                var items = new Dictionary<EntityKey, IUniqueIdentified>();

                _ = source
                    .Flatten(entity => items.TryAdd(new EntityKey(entity), entity)
                        ? UnfoldRelations(entity, modelProvider)
                        : Enumerable.Empty<IUniqueIdentified>())
                    .ToList();

                return items;
            }

            static IEnumerable<IUniqueIdentified> UnfoldRelations(
                IUniqueIdentified owner,
                IModelProvider modelProvider)
            {
                var type = owner.GetType();
                var table = modelProvider.Tables[type];

                if (table.IsMtmTable)
                {
                    return Enumerable.Empty<IUniqueIdentified>();
                }

                var relations = table
                    .Columns
                    .Values
                    .Where(column => column.IsRelation)
                    .Select(column => column.GetRelationValue(owner))
                    .Where(dependency => dependency != null)
                    .Select(dependency => dependency!);

                var multipleRelations = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .SelectMany(column => column.GetMultipleRelationValue(owner));

                var mtms = table
                    .Columns
                    .Values
                    .Where(column => column.IsMultipleRelation)
                    .SelectMany(column => column
                        .GetMultipleRelationValue(owner)
                        .Select(dependency => column.CreateMtm(owner, dependency)));

                return relations.Concat(multipleRelations).Concat(mtms);
            }

            static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> GetDependencies(
                IModelProvider modelProvider,
                IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
            {
                return entity =>
                {
                    var type = entity.GetType();
                    var table = modelProvider.Tables[type];

                    return table
                        .Columns
                        .Values
                        .Where(column => column.IsRelation)
                        .Select(DependencySelector(table, entity, map))
                        .Where(dependency => dependency != null)
                        .Select(dependency => dependency!);
                };

                static Func<ColumnInfo, IUniqueIdentified?> DependencySelector(
                    ITableInfo table,
                    IUniqueIdentified entity,
                    IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
                {
                    return column => table.IsMtmTable
                        ? map[new EntityKey(column.Relation.Target, column.GetValue(entity) !)]
                        : column.GetRelationValue(entity);
                }
            }

            static IEnumerable<SqlCommand> InsertEntity(
                ConcurrentDictionary<InsertSqlExpressionCacheKey, TranslatedSqlExpression> cache,
                ITableInfo table,
                IEnumerable<IUniqueIdentified> entities,
                EnInsertBehavior insertBehavior)
            {
                var insertExpression = cache.GetOrAdd(new InsertSqlExpressionCacheKey(table, insertBehavior), BuildInsertCommand);

                foreach (var entity in entities)
                {
                    yield return new SqlCommand(
                        insertExpression.CommandText,
                        insertExpression.CommandParametersExtractor(entity));
                }
            }

            static TranslatedSqlExpression BuildInsertCommand(
                InsertSqlExpressionCacheKey cacheKey)
            {
                var (table, insertBehavior) = cacheKey;

                var columns = table
                    .Columns
                    .Values
                    .Where(column => !column.IsMultipleRelation)
                    .ToArray();

                var columnsText = columns
                    .Select(column => ColumnFormat.Format(column.Name))
                    .ToString(", ");

                var valuesText = ValuesFormat.Format(columns
                    .Select((_, index) => $"@{TranslationContext.CommandParameterFormat.Format(index.ToString(CultureInfo.InvariantCulture))}")
                    .ToString(", "));

                var insertBehaviorText = ApplyInsertBehavior(table, insertBehavior, columns);

                var commandText = InsertCommandFormat.Format(
                    table.Schema,
                    table.Name,
                    columnsText,
                    valuesText,
                    insertBehaviorText);

                return new TranslatedSqlExpression(
                    default!,
                    commandText,
                    ValuesExtractor(columns));

                static Func<object, IReadOnlyCollection<SqlCommandParameter>> ValuesExtractor(ColumnInfo[] columns)
                {
                    return entity => columns
                        .Select((column, index) => new SqlCommandParameter(
                            TranslationContext.CommandParameterFormat.Format(index.ToString(CultureInfo.InvariantCulture)),
                            column.GetValue((IUniqueIdentified)entity),
                            column.Type,
                            column.IsJsonColumn))
                        .ToArray();
                }

                static string ApplyInsertBehavior(
                    ITableInfo table,
                    EnInsertBehavior insertBehavior,
                    ColumnInfo[] columns)
                {
                    return insertBehavior switch
                    {
                        EnInsertBehavior.Default => string.Empty,
                        EnInsertBehavior.DoNothing => OnConflictDoNothing,
                        EnInsertBehavior.DoUpdate => ApplyUpdateInsertBehavior(table, columns),
                        _ => throw new NotSupportedException(insertBehavior.ToString())
                    };

                    static string ApplyUpdateInsertBehavior(ITableInfo table, ColumnInfo[] columns)
                    {
                        return columns.All(column => column.Name.Equals(nameof(IUniqueIdentified.PrimaryKey), StringComparison.OrdinalIgnoreCase))
                               || table.IsMtmTable
                            ? OnConflictDoNothing
                            : OnConflictDoUpdate.Format(KeyColumns(table), Update(columns));

                        static string KeyColumns(ITableInfo table)
                        {
                            var uniqueIndexColumns = table
                                .Indexes
                                .Values
                                .SingleOrDefault(index => index.Unique)
                               ?.Columns.Select(column => column.Name) ?? new[] { nameof(IDatabaseEntity.PrimaryKey) };

                            return string.Join(", ", uniqueIndexColumns.Select(column => ColumnFormat.Format(column)));
                        }

                        static string Update(IEnumerable<ColumnInfo> columns)
                        {
                            return columns
                                .Select(column => AssignExpressionFormat.Format(ColumnFormat.Format(column.Name), ExcludedPseudoColumnFormat.Format(ColumnFormat.Format(column.Name))))
                                .ToString("," + Environment.NewLine);
                        }
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
            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            SqlCommand updateCommand;

            var setExpressions = new List<SqlCommand>(infos.Count);

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

                var command = _translator.Translate(info.ValueProducer);

                if (command is not SqlCommand sqlCommand)
                {
                    throw new NotSupportedException($"Unsupported command type {command.GetType()}");
                }

                setExpressions.Add(new SqlCommand(
                    AssignExpressionFormat.Format(ColumnFormat.Format(column.Name), sqlCommand.CommandText),
                    sqlCommand.CommandParameters));
            }

            {
                var command = _translator.Translate(predicate);

                if (command is not SqlCommand sqlCommand)
                {
                    throw new NotSupportedException($"Unsupported command type {command.GetType()}");
                }

                var setCommand = setExpressions.Aggregate((acc, next) => acc.Merge(next, ", "));

                var setAndPredicate = setCommand.Merge(sqlCommand, " where ");

                updateCommand = new SqlCommand(
                    UpdateCommandFormat.Format(table.Schema, table.Name, setAndPredicate.CommandText),
                    setAndPredicate.CommandParameters);
            }

            var versions = (await ((ICachedQueryable<long>)transaction
                   .All<TEntity>()
                   .Where(predicate)
                   .Select(entity => entity.Version))
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var updateVersion = await GetXid(_connectionProvider, transaction, token).ConfigureAwait(false);

            var affectedRowsCount = await _connectionProvider
                .Execute(transaction, updateCommand, token)
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
            var type = typeof(TEntity);
            var table = _modelProvider.Tables[type];

            var command = _translator.Translate(predicate);

            if (command is not SqlCommand sqlCommand)
            {
                throw new NotSupportedException($"Unsupported command type {command.GetType()}");
            }

            var deleteCommand = new SqlCommand(
                DeleteCommandFormat.Format(table.Schema, table.Name, sqlCommand.CommandText),
                sqlCommand.CommandParameters);

            var versions = (await ((ICachedQueryable<long>)transaction
                   .All<TEntity>()
                   .Where(predicate)
                   .Select(entity => entity.Version))
                   .ToListAsync(token)
                   .ConfigureAwait(false))
               .GroupBy(version => version)
               .ToDictionary(
                    grp => grp.Key,
                    grp => grp.Count());

            var affectedRowsCount = await _connectionProvider
                .Execute(transaction, deleteCommand, token)
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

        private static Task<long> GetXid(
            IDatabaseConnectionProvider connectionProvider,
            IAdvancedDatabaseTransaction transaction,
            CancellationToken token)
        {
            return connectionProvider.ExecuteScalar<long>(
                transaction,
                new SqlCommand(TransactionIdCommandText, Array.Empty<SqlCommandParameter>(), false),
                token);
        }
    }
}