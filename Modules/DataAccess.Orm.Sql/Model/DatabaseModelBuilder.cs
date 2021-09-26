namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using Api.Reading;
    using Api.Transaction;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public DatabaseModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider databaseTypeProvider,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _databaseTypeProvider = databaseTypeProvider;
            _connectionProvider = connectionProvider;
        }

        public async Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var databaseExists = await _connectionProvider
                .DoesDatabaseExist(token)
                .ConfigureAwait(false);

            if (!databaseExists)
            {
                return default;
            }

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var transaction = _dependencyContainer.Resolve<IDatabaseTransaction>();

                await using (await transaction.Open(true, token).ConfigureAwait(false))
                {
                    var entitiesShortNameMap = _databaseTypeProvider
                        .DatabaseEntities()
                        .ToDictionary(entity => entity.Name, StringComparer.OrdinalIgnoreCase);

                    var tables = (await transaction
                            .Read<DatabaseColumn, Guid>()
                            .All()
                            .GroupBy(column => column.TableName)
                            .ToDictionaryAsync(grp => grp.Key, grp => grp.ToList(), token)
                            .ConfigureAwait(false))
                        .Select(grp => BuildTableNode(grp.Key, grp.Value, entitiesShortNameMap))
                        .ToList();

                    var views = (await transaction
                            .Read<DatabaseView, Guid>()
                            .All()
                            .ToListAsync(token)
                            .ConfigureAwait(false))
                        .Select(BuildViewNode)
                        .ToList();

                    return new DatabaseNode(_connectionProvider.Database, tables, views);
                }
            }
        }

        private static TableNode BuildTableNode(string tableName,
            IReadOnlyCollection<DatabaseColumn> databaseColumns,
            IReadOnlyDictionary<string, Type> entitiesShortNameMap)
        {
            var columns = databaseColumns
                .Select(BuildColumnNode)
                .ToList();

            return entitiesShortNameMap.TryGetValue(tableName, out var entity)
                ? new TableNode(entity, columns)
                : new TableNode(tableName, columns);
        }

        private static ColumnNode BuildColumnNode(DatabaseColumn column)
        {
            var columnType = GetColumnType(column);

            return new ColumnNode(columnType, column.ColumnName);
        }

        private static Type GetColumnType(DatabaseColumn column)
        {
            throw new NotImplementedException("#110 - Model builder & migrations");
        }

        private static ViewNode BuildViewNode(DatabaseView view)
        {
            return new ViewNode(view.Name, view.Query);
        }
    }
}