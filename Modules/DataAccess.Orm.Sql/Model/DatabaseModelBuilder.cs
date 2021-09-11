namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
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
        private readonly IConnectionFactory _connectionFactory;

        public DatabaseModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider databaseTypeProvider,
            IConnectionFactory connectionFactory)
        {
            _dependencyContainer = dependencyContainer;
            _databaseTypeProvider = databaseTypeProvider;
            _connectionFactory = connectionFactory;
        }

        public async Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var databaseExists = await _connectionFactory
                .DoesDatabaseExist(token)
                .ConfigureAwait(false);

            if (!databaseExists)
            {
                return default;
            }

            var database = await _connectionFactory
                .GetDatabaseName(token)
                .ConfigureAwait(false);

            await using (_dependencyContainer.OpenScopeAsync())
            {
                var entitiesShortNameMap = _databaseTypeProvider
                    .DatabaseEntities()
                    .ToDictionary(entity => entity.Name, StringComparer.OrdinalIgnoreCase);

                var tables = _dependencyContainer
                    .Resolve<IReadRepository<DatabaseColumn, Guid>>()
                    .All()
                    .GroupBy(column => column.TableName)
                    .ToDictionary(grp => grp.Key, grp => grp.ToList())
                    .Select(grp => BuildTableNode(grp.Key, grp.Value, entitiesShortNameMap))
                    .ToList();

                var views = _dependencyContainer
                    .Resolve<IReadRepository<DatabaseView, Guid>>()
                    .All()
                    .AsEnumerable()
                    .Select(BuildViewNode)
                    .ToList();

                return new DatabaseNode(database, tables, views);
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