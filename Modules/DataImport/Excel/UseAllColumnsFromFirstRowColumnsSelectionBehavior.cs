namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Use all columns header row behavior
    /// </summary>
    [Unregistered]
    public class UseAllColumnsFromFirstRowColumnsSelectionBehavior : IExcelColumnsSelectionBehavior
    {
        private readonly IExcelCellValueExtractor _cellValueExtractor;

        /// <summary> .cctor </summary>
        /// <param name="cellValueExtractor">IExcelCellValueExtractor</param>
        public UseAllColumnsFromFirstRowColumnsSelectionBehavior(IExcelCellValueExtractor cellValueExtractor)
        {
            _cellValueExtractor = cellValueExtractor;
        }

        /// <inheritdoc />
        public bool FirstRowIsHeader { get; } = false;

        /// <inheritdoc />
        public DataColumn[] ExtractColumns(
            IEnumerable<Row> rows,
            IReadOnlyDictionary<int, string> sharedStrings,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            var row = rows.Take(1).Single();

            return row
                .Elements<Cell>()
                .Select((cell, index) => _cellValueExtractor.ExtractCellValue(cell, row.RowIndex, (uint)index, sharedStrings))
                .Select(cell => new DataColumn(cell.ColumnName)
                {
                    Caption = cell.Value
                })
                .ToArray();
        }
    }
}