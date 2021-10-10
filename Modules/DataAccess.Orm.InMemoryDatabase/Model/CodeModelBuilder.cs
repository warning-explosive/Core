namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public CodeModelBuilder(
            IDatabaseTypeProvider databaseTypeProvider,
            IDatabaseConnectionProvider connectionProvider)
        {
            _databaseTypeProvider = databaseTypeProvider;
            _connectionProvider = connectionProvider;
        }

        public Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            var schemas = _databaseTypeProvider
                .DatabaseEntities()
                .GroupBy(entity => entity.Assembly.GetName().Name)
                .Select(grp => BuildSchemaNode(grp.Key, grp))
                .ToList();

            var database = new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas);

            return Task.FromResult((DatabaseNode?)database);
        }

        private static SchemaNode BuildSchemaNode(string schema, IEnumerable<Type> entities)
        {
            var tables = entities
                .Select(entity => BuildTableNode(schema, entity))
                .ToList();

            return new SchemaNode(schema, tables, Array.Empty<ViewNode>(), Array.Empty<IndexNode>());
        }

        private static TableNode BuildTableNode(string schema, Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Select(property => BuildColumnNode(schema, tableType, property))
                .ToList();

            return new TableNode(schema, tableType.FullName, columns);

            static ColumnNode BuildColumnNode(string schema, Type tableType, PropertyInfo propertyInfo)
            {
                var tableName = tableType.Name;
                var columnName = propertyInfo.Name;
                var columnType = propertyInfo.PropertyType;
                var columnTypeName = propertyInfo.PropertyType.Name;

                return columnType.IsTypeSupported()
                    ? new ColumnNode(schema, tableName, columnName, columnTypeName, Array.Empty<string>())
                    : throw new NotSupportedException($"Not supported column type: {tableName}.{columnName} - {columnTypeName}");
            }
        }
    }
}