namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Settings;
    using Settings;
    using Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder,
                                      IResolvable<ICodeModelBuilder>
    {
        private readonly ISettingsProvider<SqlDatabaseSettings> _settingsProvider;
        private readonly IModelProvider _modelProvider;
        private readonly IColumnDataTypeProvider _columnDataTypeProvider;

        public CodeModelBuilder(
            ISettingsProvider<SqlDatabaseSettings> settingsProvider,
            IModelProvider modelProvider,
            IColumnDataTypeProvider columnDataTypeProvider)
        {
            _settingsProvider = settingsProvider;
            _modelProvider = modelProvider;
            _columnDataTypeProvider = columnDataTypeProvider;
        }

        public async Task<DatabaseNode?> BuildModel(IReadOnlyCollection<Type> databaseEntities, CancellationToken token)
        {
            var tables = databaseEntities
                .Where(type => _modelProvider.Tables.ContainsKey(type))
                .Select(type => _modelProvider.Tables[type]);

            var mtmTables = _modelProvider
                .Tables
                .Values
                .OfType<MtmTableInfo>()
                .Where(info => info.Columns.Any(column => databaseEntities.Contains(column.Value.Relation.Target)));

            var schemas = tables
                .Concat(mtmTables)
                .GroupBy(info => info.Schema)
                .Select(schema => BuildSchemaNode(schema.Key, schema))
                .ToArray();

            var settings = await _settingsProvider
                .Get(token)
                .ConfigureAwait(false);

            return new DatabaseNode(settings.Host, settings.Database, schemas);
        }

        private SchemaNode BuildSchemaNode(
            string schema,
            IEnumerable<ITableInfo> objects)
        {
            var enumTypes = _modelProvider
                .Enums
                .Where(info => info.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase))
                .Select(info => new EnumTypeNode(info.Schema, info.Type.Name, info.Type.GetEnumNames()))
                .ToList();

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

            return new SchemaNode(schema, enumTypes, tables, views, indexes);
        }

        private TableNode BuildTableNode(TableInfo tableInfo)
        {
            var columns = tableInfo
                .Columns
                .Select(column => BuildColumnNode(column.Value))
                .ToList();

            return new TableNode(tableInfo.Schema, tableInfo.Name, columns);
        }

        private ColumnNode BuildColumnNode(ColumnInfo columnInfo)
        {
            var dataType = _columnDataTypeProvider.GetColumnDataType(columnInfo);

            return new ColumnNode(columnInfo.Table.Schema, columnInfo.Table.Name, columnInfo.Name, dataType, columnInfo.Constraints);
        }

        private static ViewNode BuildViewNode(ViewInfo viewInfo)
        {
            return new ViewNode(viewInfo.Schema, viewInfo.Name, viewInfo.Query);
        }

        private static IndexNode BuildIndexNode(IndexInfo indexInfo)
        {
            var columns = indexInfo
                .Columns
                .Select(column => column.Name)
                .ToList();

            return new IndexNode(indexInfo.Table.Schema, indexInfo.Table.Name, columns, indexInfo.Unique);
        }
    }
}