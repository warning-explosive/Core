namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
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
    using CompositionRoot.Api.Abstractions.Container;
    using Connection;
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
            var schemas = _databaseTypeProvider
                .DatabaseEntities()
                .GroupBy(entity => entity.SchemaName())
                .Select(grp => BuildSchemaNode(grp.Key, grp))
                .ToArray();

            return Task.FromResult((DatabaseNode?)new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas));
        }

        private SchemaNode BuildSchemaNode(string schema, IEnumerable<Type> entities)
        {
            var tables = new List<TableNode>();
            var views = new List<ViewNode>();

            foreach (var entity in entities)
            {
                if (entity.IsSqlView())
                {
                    views.Add(BuildViewNode(entity));
                }
                else
                {
                    tables.Add(BuildTableNode(entity));
                }
            }

            return new SchemaNode(schema, tables, views);
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

        private ViewNode BuildViewNode(Type viewType)
        {
            return new ViewNode(viewType, viewType.SqlViewQuery(_dependencyContainer));
        }
    }
}