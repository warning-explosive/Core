namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Connection;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IDatabaseConnectionProvider _connectionProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CodeModelBuilder(
            IDatabaseTypeProvider databaseTypeProvider,
            IModelProvider modelProvider,
            IDatabaseConnectionProvider connectionProvider,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            _databaseTypeProvider = databaseTypeProvider;
            _modelProvider = modelProvider;
            _connectionProvider = connectionProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            return BuildModel(_databaseTypeProvider.DatabaseEntities().ToArray(), token);
        }

        public Task<DatabaseNode?> BuildModel(Type[] databaseEntities, CancellationToken token)
        {
            var schemas = _modelProvider
                .ObjectsFor(databaseEntities)
                .Select(grp => BuildSchemaNode(grp.Key, grp.Value.Values.ToList()))
                .ToArray();

            return Task.FromResult((DatabaseNode?)new DatabaseNode(_connectionProvider.Host, _connectionProvider.Database, schemas));
        }

        private SchemaNode BuildSchemaNode(string schema, IReadOnlyCollection<IObjectModelInfo> objects)
        {
            var tables = new List<TableNode>();
            var views = new List<ViewNode>();
            var indexes = new List<IndexNode>();

            foreach (var obj in objects)
            {
                if (obj is TableInfo tableInfo)
                {
                    tables.Add(BuildTableNode(tableInfo));
                }
                else if (obj is ViewInfo viewInfo)
                {
                    views.Add(BuildViewNode(viewInfo));
                }

                indexes.AddRange(obj.Indexes.Select(index => BuildIndexNode(index.Value)));
            }

            return new SchemaNode(schema, tables, views, indexes);
        }

        private TableNode BuildTableNode(TableInfo tableInfo)
        {
            var columns = tableInfo
                .Columns
                .Select(column => BuildColumnNode(column.Value))
                .ToList();

            return new TableNode(tableInfo.Schema, tableInfo.Type.TableName(), columns);
        }

        private ColumnNode BuildColumnNode(ColumnInfo columnInfo)
        {
            var dataType = _columnDataTypeProvider.GetColumnDataType(columnInfo.Type);

            return new ColumnNode(columnInfo.Schema, columnInfo.Table.TableName(), columnInfo.Name, dataType, columnInfo.Constraints);
        }

        private static ViewNode BuildViewNode(ViewInfo viewInfo)
        {
            return new ViewNode(viewInfo.Schema, viewInfo.Type.TableName(), viewInfo.Query);
        }

        private static IndexNode BuildIndexNode(IndexInfo indexInfo)
        {
            var columns = indexInfo
                .Columns
                .Select(column => column.Name)
                .ToList();

            return new IndexNode(indexInfo.Schema, indexInfo.Table.TableName(), columns, indexInfo.Unique);
        }
    }
}