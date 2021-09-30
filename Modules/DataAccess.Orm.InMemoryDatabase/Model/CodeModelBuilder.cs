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
                .GroupBy(entity => entity.SchemaName())
                .Select(grp => BuildSchemaNode(grp.Key, grp))
                .ToList();

            var database = new DatabaseNode(_connectionProvider.Database, schemas);

            return Task.FromResult((DatabaseNode?)database);
        }

        private static SchemaNode BuildSchemaNode(string schema, IEnumerable<Type> entities)
        {
            var tables = entities
                .Select(BuildTableNode)
                .ToList();

            return new SchemaNode(schema, tables, Array.Empty<ViewNode>());
        }

        private static TableNode BuildTableNode(Type tableType)
        {
            var columns = tableType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
                .Select(property => BuildColumnNode(tableType, property))
                .ToList();

            return new TableNode(tableType, columns);
            static ColumnNode BuildColumnNode(Type tableType, PropertyInfo propertyInfo)
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
}