namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Orm.Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CodeModelBuilder(
            IDependencyContainer dependencyContainer,
            IDatabaseTypeProvider databaseTypeProvider,
            IDatabaseConnectionProvider connectionProvider)
        {
            _dependencyContainer = dependencyContainer;
            _databaseTypeProvider = databaseTypeProvider;
            _connectionProvider = connectionProvider;
        }

        public Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var tables = _databaseTypeProvider
                .DatabaseEntities()
                .Select(BuildTableNode)
                .ToList();

            var database = new DatabaseNode(_connectionProvider.Database, tables, Array.Empty<ViewNode>());

            return Task.FromResult((DatabaseNode?)database);
        }

        private static TableNode BuildTableNode(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Select(property => BuildColumnNode(tableType, property))
                .ToList();

            return new TableNode(tableType, columns);
        }

        private static ColumnNode BuildColumnNode(Type tableType, PropertyInfo propertyInfo)
        {
            var tableName = tableType.Name;
            var columnType = propertyInfo.PropertyType;
            var columnName = propertyInfo.Name;

            return columnType.IsTypeSupported()
                ? new ColumnNode(columnType, columnName)
                : throw new NotSupportedException($"Not supported column type: {tableName}.{columnName} - {columnType}");
        }
    }
}