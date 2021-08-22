namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Contract.Abstractions;
    using GenericDomain.Abstractions;
    using Orm.Connection;
    using Orm.Model;
    using Orm.Model.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelBuilder : IDatabaseModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDomainTypeProvider _domainTypeProvider;
        private readonly IConnectionFactory _connectionFactory;

        public DatabaseModelBuilder(
            IDependencyContainer dependencyContainer,
            IDomainTypeProvider domainTypeProvider,
            IConnectionFactory connectionFactory)
        {
            _dependencyContainer = dependencyContainer;
            _domainTypeProvider = domainTypeProvider;
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
                var entitiesShortNameMap = _domainTypeProvider
                    .Entities()
                    .ToDictionary(entity => entity.Name, StringComparer.OrdinalIgnoreCase);

                var tables = _dependencyContainer
                    .Resolve<IReadRepository<DatabaseColumn>>()
                    .All()
                    .GroupBy(column => column.TableName)
                    .ToDictionary(grp => grp.Key, grp => grp.ToList())
                    .Select(grp => BuildTableNode(grp.Key, grp.Value, entitiesShortNameMap))
                    .ToList();

                var views = _dependencyContainer
                    .Resolve<IReadRepository<DatabaseView>>()
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
            throw new NotImplementedException();
        }

        private static ViewNode BuildViewNode(DatabaseView view)
        {
            return new ViewNode(view.Name, view.Query);
        }
    }
}