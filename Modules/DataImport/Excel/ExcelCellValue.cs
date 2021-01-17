namespace SpaceEngineers.Core.DataImport.Excel
{
    /// <summary>
    /// Cell value
    /// </summary>
    public class ExcelCellValue
    {
        /// <summary> .cctor </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnIndex">Column index</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value</param>
        internal ExcelCellValue(uint rowIndex, uint columnIndex, string columnName, string? value)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            ColumnName = columnName;
            Value = value;
        }

        /// <summary>
        /// Row index
        /// </summary>
        internal uint RowIndex { get; }

        /// <summary>
        /// Column index
        /// </summary>
        internal uint ColumnIndex { get; }

        /// <summary>
        /// Column name
        /// </summary>
        internal string ColumnName { get; }

        /// <summary>
        /// Value
        /// </summary>
        internal string? Value { get; }
    }
}