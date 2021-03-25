namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Use all columns from first row columns selection behavior
    /// </summary>
    [Lifestyle(EnLifestyle.Singleton)]
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
        public bool FirstRowIsHeader { get; } = true;

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