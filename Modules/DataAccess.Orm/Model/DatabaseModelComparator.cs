namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelComparator : IDatabaseModelComparator
    {
        public IEnumerable<IDatabaseModelChange> ExtractDiff(DatabaseNode? actualModel, DatabaseNode? expectedModel)
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
        }

        private static IEnumerable<IDatabaseModelChange> ExtractSchemasDiff(IEnumerable<SchemaNode> actualModel, IEnumerable<SchemaNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Name,
                    expected => expected.Name,
                    SchemaChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IDatabaseModelChange> SchemaChangesSelector(SchemaNode? actualSchema, SchemaNode? expectedSchema)
        {
            if (actualSchema != null && expectedSchema != null)
            {
                foreach (var tableChanges in ExtractTablesDiff(actualSchema.Tables, expectedSchema.Tables))
                {
                    yield return tableChanges;
                }

                foreach (var viewDiff in ExtractViewsDiff(actualSchema.Views, expectedSchema.Views))
                {
                    yield return viewDiff;
                }
            }
            else if (actualSchema != null && expectedSchema == null)
            {
                throw new InvalidOperationException($"You can't automatically drop the schema {actualSchema.Name}");
            }
            else if (actualSchema == null && expectedSchema != null)
            {
                yield return new CreateSchema(expectedSchema.Name);

                foreach (var tableChanges in ExtractTablesDiff(Enumerable.Empty<TableNode>(), expectedSchema.Tables))
                {
                    yield return tableChanges;
                }

                foreach (var viewDiff in ExtractViewsDiff(Enumerable.Empty<ViewNode>(), expectedSchema.Views))
                {
                    yield return viewDiff;
                }
            }
        }

        private static IEnumerable<IDatabaseModelChange> ExtractTablesDiff(IEnumerable<TableNode> actualModel, IEnumerable<TableNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Name,
                    expected => expected.Name,
                    TableChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IDatabaseModelChange> TableChangesSelector(TableNode? actualTable, TableNode? expectedTable)
        {
            if (actualTable != null && expectedTable != null)
            {
                foreach (var columnChange in ExtractColumnsDiff(actualTable, expectedTable))
                {
                    yield return columnChange;
                }
            }
            else if (actualTable != null && expectedTable == null)
            {
                yield return new DropTable(actualTable);
            }
            else if (actualTable == null && expectedTable != null && expectedTable.Type != null)
            {
                yield return new CreateTable(expectedTable);

                var emptyTable = new TableNode(expectedTable.Type, new List<ColumnNode>());

                foreach (var columnChange in ExtractColumnsDiff(emptyTable, expectedTable))
                {
                    yield return columnChange;
                }
            }
            else
            {
                throw new InvalidOperationException("Wrong database table change");
            }
        }

        private static IEnumerable<IDatabaseModelChange> ExtractColumnsDiff(TableNode actualTable, TableNode expectedTable)
        {
            return actualTable.Columns
                .FullOuterJoin(expectedTable.Columns,
                    actual => actual.Name,
                    expected => expected.Name,
                    ColumnChangesSelector(actualTable, expectedTable),
                    StringComparer.OrdinalIgnoreCase);
        }

        private static Func<ColumnNode?, ColumnNode?, IDatabaseModelChange> ColumnChangesSelector(TableNode actualTable, TableNode expectedTable)
        {
            return (actualColumn, expectedColumn) =>
            {
                if (actualColumn != null && expectedColumn != null)
                {
                    return new AlterColumn(actualTable, expectedTable, actualColumn, expectedColumn);
                }
                else if (actualColumn != null && expectedColumn == null)
                {
                    return new DropColumn(actualTable, actualColumn);
                }
                else if (actualColumn == null && expectedColumn != null)
                {
                    return new AddColumn(expectedTable, expectedColumn);
                }
                else
                {
                    throw new InvalidOperationException("Wrong database column change");
                }
            };
        }

        private static IEnumerable<IDatabaseModelChange> ExtractViewsDiff(IEnumerable<ViewNode> actualModel, IEnumerable<ViewNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Name,
                    expected => expected.Name,
                    ViewChangesSelector,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(change => change);

            foreach (var modelChange in modelChanges)
            {
                yield return modelChange;
            }
        }

        private static IEnumerable<IDatabaseModelChange> ViewChangesSelector(ViewNode? actualView, ViewNode? expectedView)
        {
            if (actualView != null && expectedView != null && expectedView.Type != null)
            {
                yield return new UpsertView(expectedView.Type, expectedView.Query);
            }
            else if (actualView != null && expectedView == null)
            {
                yield return new DropView(actualView.Name);
            }
            else if (actualView == null && expectedView != null && expectedView.Type != null)
            {
                yield return new UpsertView(expectedView.Type, expectedView.Query);
            }
            else
            {
                throw new InvalidOperationException("Wrong database view change");
            }
        }
    }
}