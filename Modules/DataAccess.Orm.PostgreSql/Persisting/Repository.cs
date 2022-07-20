namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Persisting
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Persisting;
    using Api.Transaction;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using CrossCuttingConcerns.Settings;
    using DataAccess.Orm.Extensions;
    using Orm.Connection;
    using Settings;
    using Sql.Extensions;
    using Sql.Model;
    using Sql.Translation.Extensions;
    using Transaction;

    [SuppressMessage("Analysis", "CA1506", Justification = "Infrastructural code")]
    [Component(EnLifestyle.Scoped)]
    internal class Repository : IRepository,
                                IResolvable<IRepository>
    {
        private const string InsertQueryFormat = @"insert into ""{0}"".""{1}""({2}) values {3}{4}";
        private const string OnConflictDoNothing = @" on conflict do nothing";
        private const string OnConflictDoUpdate = @" on conflict (""PrimaryKey"") do update set {0}";
        private const string SetExpressionFormat = @"{0} = {1}";
        private const string ValuesFormat = @"({0})";
        private const string ColumnFormat = @"""{0}""";
        private const string ExcludedPseudoColumnFormat = @"excluded.{0}";

        private readonly IDependencyContainer _dependencyContainer;
        private readonly IAdvancedDatabaseTransaction _transaction;
        private readonly IModelProvider _modelProvider;
        private readonly ISettingsProvider<OrmSettings> _settingsProvider;
        private readonly IDatabaseProvider _databaseProvider;

        public Repository(
            IDependencyContainer dependencyContainer,
            IAdvancedDatabaseTransaction transaction,
            IModelProvider modelProvider,
            ISettingsProvider<OrmSettings> settingsProvider,
            IDatabaseProvider databaseProvider)
        {
            _dependencyContainer = dependencyContainer;
            _transaction = transaction;
            _modelProvider = modelProvider;
            _settingsProvider = settingsProvider;
            _databaseProvider = databaseProvider;
        }

        public async Task<long> Insert(
            IUniqueIdentified[] entities,
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

            var version = await _transaction
               .GetXid(settings, token)
               .ConfigureAwait(false);

            IReadOnlyDictionary<object, IUniqueIdentified> map = entities
               .SelectMany(entity => entity.Flatten(_modelProvider))
               .DistinctBy(GetKey)
               .ToDictionary(GetKey);

            foreach (var entity in map.Values)
            {
                var type = entity.GetType();

                if (!type.IsSubclassOfOpenGeneric(typeof(BaseMtmDatabaseEntity<,>)))
                {
                    type
                       .GetProperty(nameof(IDatabaseEntity<Guid>.Version), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty)
                       .EnsureNotNull($"Database entity {type} should have column {nameof(IDatabaseEntity<Guid>.Version)}")
                       .SetValue(entity, version);
                }
            }

            var commandText = map
               .Values
               .OrderByDependencies(GetKey, GetDependencies(_modelProvider, map))
               .Stack(entity => entity.GetType())
               .Select(grp => InsertEntity(grp.Key, grp.Value, _dependencyContainer, _modelProvider, insertBehavior))
               .ToString(";" + Environment.NewLine);

            var affectedRowsCount = await ExecutionExtensions
               .TryAsync((commandText, settings), _transaction.InvokeScalar)
               .Catch<Exception>()
               .Invoke(_databaseProvider.Handle<long>(commandText), token)
               .ConfigureAwait(false);

            var change = new CreateEntityChange(entities, insertBehavior, affectedRowsCount);

            _transaction.CollectChange(change);

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