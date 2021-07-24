namespace SpaceEngineers.Core.DataAccess.Orm.Model.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelComparator : IDatabaseModelComparator
    {
        public IEnumerable<IDatabaseModelChange> ExtractDiff(DatabaseNode? actualModel, DatabaseNode? expectedModel)
        {
            if (actualModel != null && expectedModel != null)
            {
                foreach (var tableChanges in ExtractTablesDiff(actualModel.Tables, expectedModel.Tables))
                {
                    yield return tableChanges;
                }
            }
            else if (actualModel != null && expectedModel == null)
            {
                throw new InvalidOperationException("You can't drop the database automatically");
            }
            else if (actualModel == null && expectedModel != null)
            {
                yield return new CreateDatabase(expectedModel.Name);

                foreach (var tableChanges in ExtractTablesDiff(Enumerable.Empty<TableNode>(), expectedModel.Tables))
                {
                    yield return tableChanges;
                }
            }
        }

        public IEnumerable<IDatabaseModelChange> ExtractTablesDiff(IEnumerable<TableNode> actualModel, IEnumerable<TableNode> expectedModel)
        {
            var modelChanges = actualModel
                .FullOuterJoin(expectedModel,
                    actual => actual.Type,
                    expected => expected.Type,
                    TableChangesSelector)
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
            else if (actualTable == null && expectedTable != null)
            {
                yield return new CreateTable(expectedTable);

                var emptyTable = new TableNode(expectedTable.Type, expectedTable.Name, new List<ColumnNode>());

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
                    ColumnChangesSelector(actualTable, expectedTable));
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
    }
}