namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangesSorter : IModelChangesSorter,
                                        IResolvable<IModelChangesSorter>
    {
        private readonly IModelProvider _modelProvider;

        public ModelChangesSorter(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public IOrderedEnumerable<IModelChange> Sort(IEnumerable<IModelChange> source)
        {
            var materialized = source.ToList();

            return materialized
                .OrderByDependencies(GetKey, GetDependencies(materialized))
                .ThenBy(modelChange => modelChange.ToString());

            static string GetKey(IModelChange modelChange)
            {
                return modelChange.ToString();
            }

            Func<IModelChange, IEnumerable<IModelChange>> GetDependencies(IReadOnlyCollection<IModelChange> changes)
            {
                return modelChange =>
                {
                    switch (modelChange)
                    {
                        case CreateDatabase:
                            return Enumerable.Empty<IModelChange>();
                        case CreateSchema createSchema:
                            return changes
                                .OfType<CreateDatabase>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<CreateTable>()
                                    .Where(createTable => createTable.Schema.Equals(createSchema.Schema, StringComparison.OrdinalIgnoreCase))
                                    .Select(createTable => _modelProvider.TablesMap[createTable.Schema][createTable.Table])
                                    .OfType<TableInfo>()
                                    .SelectMany(tableInfo => tableInfo.Columns.Values)
                                    .Where(columnInfo => columnInfo.Relation != null)
                                    .Select(columnInfo => _modelProvider.SchemaName(columnInfo.Relation!.Target))
                                    .Where(schema => !schema.Equals(createSchema.Schema, StringComparison.OrdinalIgnoreCase))
                                    .SelectMany(schema => changes
                                        .OfType<CreateSchema>()
                                        .Where(change => change.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase))));
                        case CreateTable createTable:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<DropTable>())
                                .Concat(GetTableDependencies(_modelProvider, createTable)
                                    .SelectMany(dependency => changes
                                        .OfType<CreateTable>()
                                        .Where(change => change.Schema.Equals(_modelProvider.SchemaName(dependency), StringComparison.OrdinalIgnoreCase)
                                                         && change.Table.Equals(_modelProvider.TableName(dependency), StringComparison.OrdinalIgnoreCase)
                                                         && GetTableDependencies(_modelProvider, change).All(cycleDependency => cycleDependency != _modelProvider.TablesMap[createTable.Schema][createTable.Table].Type))));
                        case CreateView:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<CreateTable>())
                                .Concat(changes
                                    .OfType<DropView>());
                        case CreateIndex:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<CreateTable>())
                                .Concat(changes
                                    .OfType<CreateView>());
                        case CreateColumn createColumn:
                            var dependency = GetColumnDependency(createColumn);
                            return dependency == null
                                ? Enumerable.Empty<IModelChange>()
                                : changes
                                    .OfType<CreateTable>()
                                    .Where(change => change.Schema.Equals(_modelProvider.SchemaName(dependency), StringComparison.OrdinalIgnoreCase)
                                                     && change.Table.Equals(_modelProvider.TableName(dependency), StringComparison.OrdinalIgnoreCase));
                        case DropIndex:
                        case DropTable:
                        case DropView:
                        case DropColumn:
                        case AlterColumn:
                            return Enumerable.Empty<IModelChange>();
                        default:
                            throw new NotSupportedException($"Not supported model change: {modelChange}");
                    }
                };
            }

            static IEnumerable<Type> GetTableDependencies(
                IModelProvider modelProvider,
                CreateTable createTable)
            {
                return modelProvider
                    .TablesMap[createTable.Schema][createTable.Table]
                    .Columns
                    .Values
                    .Where(column => column.Relation != null)
                    .Select(column => column.Relation!.Target);
            }

            Type? GetColumnDependency(CreateColumn createColumn)
            {
                return _modelProvider
                    .TablesMap[createColumn.Schema][createColumn.Table]
                    .Columns[createColumn.Column]
                    .Relation
                   ?.Target;
            }
        }
    }
}