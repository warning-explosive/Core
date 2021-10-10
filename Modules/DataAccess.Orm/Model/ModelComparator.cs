namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class ModelComparator : IModelComparator
    {
        public IEnumerable<IModelChange> ExtractDiff(DatabaseNode? actualModel, DatabaseNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                foreach (var schemaChanges in ExtractSchemasDiff(actualModel.Schemas, expectedModel.Schemas))
                {
                    yield return schemaChanges;
                }
            }
            else if (actualModel != null && expectedModel == null)
            {
                throw new InvalidOperationException($"You can't automatically drop the database {actualModel.Name}");
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateDatabase(expectedModel.Name);

                foreach (var schemaChanges in ExtractSchemasDiff(Enumerable.Empty<SchemaNode>(), expectedModel.Schemas))
                {
                    yield return schemaChanges;
                }
            }
            else
            {
                throw new InvalidOperationException("Wrong database change");
            }
        }

        private static IEnumerable<IModelChange> ExtractSchemasDiff(IEnumerable<SchemaNode> actualModel, IEnumerable<SchemaNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Schema,
                    expected => expected.Schema,
                    SchemaChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IModelChange> SchemaChangesSelector(SchemaNode? actualModel, SchemaNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                foreach (var tableChanges in ExtractTablesDiff(actualModel.Tables, expectedModel.Tables))
                {
                    yield return tableChanges;
                }

                foreach (var viewDiff in ExtractViewsDiff(actualModel.Views, expectedModel.Views))
                {
                    yield return viewDiff;
                }

                foreach (var viewDiff in ExtractIndexesDiff(actualModel.Indexes, expectedModel.Indexes))
                {
                    yield return viewDiff;
                }
            }
            else if (actualModel != null && expectedModel == null)
            {
                throw new InvalidOperationException($"You can't automatically drop the schema {actualModel.Schema}");
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateSchema(expectedModel.Schema);

                foreach (var tableChanges in ExtractTablesDiff(Enumerable.Empty<TableNode>(), expectedModel.Tables))
                {
                    yield return tableChanges;
                }

                foreach (var viewDiff in ExtractViewsDiff(Enumerable.Empty<ViewNode>(), expectedModel.Views))
                {
                    yield return viewDiff;
                }

                foreach (var viewDiff in ExtractIndexesDiff(Enumerable.Empty<IndexNode>(), expectedModel.Indexes))
                {
                    yield return viewDiff;
                }
            }
        }

        private static IEnumerable<IModelChange> ExtractTablesDiff(IEnumerable<TableNode> actualModel, IEnumerable<TableNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Table,
                    expected => expected.Table,
                    TableChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IModelChange> TableChangesSelector(TableNode? actualModel, TableNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                foreach (var columnChange in ExtractColumnsDiff(actualModel, expectedModel))
                {
                    yield return columnChange;
                }
            }
            else if (actualModel != null && expectedModel == null)
            {
                yield return new DropTable(actualModel.Schema, actualModel.Table);
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateTable(expectedModel.Schema, expectedModel.Table);
            }
            else
            {
                throw new InvalidOperationException("Wrong database table change");
            }
        }

        private static IEnumerable<IModelChange> ExtractColumnsDiff(TableNode actualModel, TableNode expectedModel)
        {
            return actualModel.Columns
                .FullOuterJoin(expectedModel.Columns,
                    actual => actual.Column,
                    expected => expected.Column,
                    ColumnChangesSelector,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static IModelChange ColumnChangesSelector(ColumnNode? actualColumn, ColumnNode? expectedModel)
        {
            if (actualColumn != null && expectedModel != null)
            {
                return new AlterColumn(expectedModel.Schema, expectedModel.Table, expectedModel.Column);
            }

            if (actualColumn != null && expectedModel == null)
            {
                return new DropColumn(actualColumn.Schema, actualColumn.Table, actualColumn.Column);
            }

            if (actualColumn == null && expectedModel != null)
            {
                return new CreateColumn(expectedModel.Schema, expectedModel.Table, expectedModel.Column);
            }

            throw new InvalidOperationException("Wrong database column change");
        }

        private static IEnumerable<IModelChange> ExtractViewsDiff(IEnumerable<ViewNode> actualModel, IEnumerable<ViewNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.View,
                    expected => expected.View,
                    ViewChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IModelChange> ViewChangesSelector(ViewNode? actualModel, ViewNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                yield return new DropView(actualModel.Schema, actualModel.View);
                yield return new CreateView(expectedModel.Schema, expectedModel.View);
            }
            else if (actualModel != null && expectedModel == null)
            {
                yield return new DropView(actualModel.Schema, actualModel.View);
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateView(expectedModel.Schema, expectedModel.View);
            }
            else
            {
                throw new InvalidOperationException("Wrong database view change");
            }
        }

        private static IEnumerable<IModelChange> ExtractIndexesDiff(IEnumerable<IndexNode> actualModel, IEnumerable<IndexNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.ToString(),
                    expected => expected.ToString(),
                    IndexChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IModelChange> IndexChangesSelector(IndexNode? actualModel, IndexNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                yield return new DropIndex(actualModel.Schema, actualModel.Table, actualModel.ToString());
                yield return new CreateIndex(expectedModel.Schema, expectedModel.Table, expectedModel.ToString());
            }
            else if (actualModel != null && expectedModel == null)
            {
                yield return new DropIndex(actualModel.Schema, actualModel.Table, actualModel.ToString());
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateIndex(expectedModel.Schema, expectedModel.Table, expectedModel.ToString());
            }
            else
            {
                throw new InvalidOperationException("Wrong database index change");
            }
        }
    }
}