namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Model;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangesSorter : IModelChangesSorter
    {
        private readonly IModelProvider _modelProvider;

        public ModelChangesSorter(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        public IOrderedEnumerable<IModelChange> Sort(IEnumerable<IModelChange> source)
        {
            var materialized = source.ToList();

            return materialized.OrderByDependencies(GetKey, GetDependencies(materialized));

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
                                    .Select(createTable => _modelProvider.Objects[createTable.Schema][createTable.Table])
                                    .OfType<TableInfo>()
                                    .SelectMany(tableInfo => tableInfo.Columns.Values)
                                    .Where(columnInfo => columnInfo.Relation != null)
                                    .Select(columnInfo => columnInfo.Relation!.Type.SchemaName())
                                    .Where(schema => !schema.Equals(createSchema.Schema, StringComparison.OrdinalIgnoreCase))
                                    .SelectMany(schema => changes
                                        .OfType<CreateSchema>()
                                        .Where(change => change.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase))));
                        case CreateTable createTable:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(_modelProvider
                                    .Objects[createTable.Schema][createTable.Table]
                                    .Columns
                                    .Values
                                    .Where(column => column.Relation != null)
                                    .Select(column => column.Relation!.Type)
                                    .SelectMany(dependency => changes
                                        .OfType<CreateTable>()
                                        .Where(change => change.Schema.Equals(dependency.SchemaName(), StringComparison.OrdinalIgnoreCase)
                                                         && change.Table.Equals(dependency.Name, StringComparison.OrdinalIgnoreCase))));
                        case CreateView:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<CreateTable>());
                        case CreateIndex:
                            return changes
                                .OfType<CreateSchema>()
                                .Cast<IModelChange>()
                                .Concat(changes
                                    .OfType<CreateTable>())
                                .Concat(changes
                                    .OfType<CreateView>());
                        case CreateColumn createColumn:
                            var dependency = _modelProvider
                                .Objects[createColumn.Schema][createColumn.Table]
                                .Columns[createColumn.Column]
                                .Relation
                               ?.Type;
                            return dependency != null
                                ? changes
                                    .OfType<CreateTable>()
                                    .Where(change => change.Schema.Equals(dependency.SchemaName(), StringComparison.OrdinalIgnoreCase)
                                                     && change.Table.Equals(dependency.Name, StringComparison.OrdinalIgnoreCase))
                                : Enumerable.Empty<IModelChange>();
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
        }
    }
}