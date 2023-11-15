namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Settings;
    using SpaceEngineers.Core.CrossCuttingConcerns.Settings;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model.Attributes;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder,
                                      IResolvable<ICodeModelBuilder>
    {
        private readonly SqlDatabaseSettings _sqlDatabaseSettings;
        private readonly IModelProvider _modelProvider;
        private readonly IDatabaseFunctionProvider<AppendOnlyAttribute> _appendOnlyFunctionProvider;

        public CodeModelBuilder(
            ISettingsProvider<SqlDatabaseSettings> sqlDatabaseSettingsProvider,
            IModelProvider modelProvider,
            IDatabaseFunctionProvider<AppendOnlyAttribute> appendOnlyFunctionProvider)
        {
            _sqlDatabaseSettings = sqlDatabaseSettingsProvider.Get();

            _modelProvider = modelProvider;
            _appendOnlyFunctionProvider = appendOnlyFunctionProvider;
        }

        public Task<DatabaseNode?> BuildModel(IReadOnlyCollection<Type> databaseEntities, CancellationToken token)
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

            var databaseNode = new DatabaseNode(_sqlDatabaseSettings.Host, _sqlDatabaseSettings.Database, schemas);

            return Task.FromResult<DatabaseNode?>(databaseNode);
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
            var functions = new List<FunctionNode>();
            var triggers = new List<TriggerNode>();

            foreach (var obj in objects)
            {
                if (obj is TableInfo table)
                {
                    tables.Add(BuildTableNode(table));

                    if (table.Type.HasAttribute<AppendOnlyAttribute>())
                    {
                        functions.Add(new FunctionNode(schema, nameof(AppendOnlyAttribute), _appendOnlyFunctionProvider.GetDefinition(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["schema"] = schema })));
                        triggers.Add(new TriggerNode(schema, $"{table.Name}_aotrg", table.Name, nameof(AppendOnlyAttribute), EnTriggerType.Before, EnTriggerEvent.Update | EnTriggerEvent.Delete));
                    }
                }
                else if (obj is ViewInfo view)
                {
                    views.Add(BuildViewNode(view));
                }

                indexes.AddRange(obj.Indexes.Select(index => BuildIndexNode(index.Value)));
            }

            return new SchemaNode(schema, enumTypes, tables, views, indexes, functions, triggers);
        }

        private static TableNode BuildTableNode(TableInfo tableInfo)
        {
            var columns = tableInfo
                .Columns
                .Select(column => BuildColumnNode(column.Value))
                .ToList();

            return new TableNode(
                tableInfo.Schema,
                tableInfo.Name,
                columns);
        }

        private static ColumnNode BuildColumnNode(ColumnInfo columnInfo)
        {
            return new ColumnNode(
                columnInfo.Table.Schema,
                columnInfo.Table.Name,
                columnInfo.Name,
                columnInfo.DataType,
                columnInfo.Constraints);
        }

        private static ViewNode BuildViewNode(ViewInfo viewInfo)
        {
            return new ViewNode(
                viewInfo.Schema,
                viewInfo.Name,
                viewInfo.Query);
        }

        private static IndexNode BuildIndexNode(IndexInfo indexInfo)
        {
            var columns = indexInfo
                .Columns
                .Select(column => column.Name)
                .ToList();

            var includedColumns = indexInfo
                .IncludedColumns
                .Select(column => column.Name)
                .ToList();

            return new IndexNode(
                indexInfo.Table.Schema,
                indexInfo.Table.Name,
                columns,
                includedColumns,
                indexInfo.Unique,
                indexInfo.Predicate);
        }
    }
}