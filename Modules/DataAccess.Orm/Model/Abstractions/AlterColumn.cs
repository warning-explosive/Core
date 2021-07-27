namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    /// <summary>
    /// Alter column change
    /// </summary>
    public class AlterColumn : IDatabaseModelChange
    {
        /// <summary> .cctor </summary>
        /// <param name="actualTable">Actual table</param>
        /// <param name="expectedTable">Expected table</param>
        /// <param name="actualColumn">Actual column</param>
        /// <param name="expectedColumn">Expected column</param>
        public AlterColumn(
            TableNode actualTable,
            TableNode expectedTable,
            ColumnNode actualColumn,
            ColumnNode expectedColumn)
        {
            ActualTable = actualTable;
            ExpectedTable = expectedTable;
            ActualColumn = actualColumn;
            ExpectedColumn = expectedColumn;
        }

        /// <summary>
        /// Actual table
        /// </summary>
        public TableNode ActualTable { get; }

        /// <summary>
        /// Expected table
        /// </summary>
        public TableNode ExpectedTable { get; }

        /// <summary>
        /// Actual column
        /// </summary>
        public ColumnNode ActualColumn { get; }

        /// <summary>
        /// Expected column
        /// </summary>
        public ColumnNode ExpectedColumn { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(AlterColumn)} {ActualTable.Type.Name}.{ActualColumn.Name}";
        }
    }
}