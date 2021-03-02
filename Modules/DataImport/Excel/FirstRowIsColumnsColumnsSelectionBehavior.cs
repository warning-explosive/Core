namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// First row is columns header header row behavior
    /// </summary>
    [Unregistered]
    public class FirstRowIsColumnsColumnsSelectionBehavior : IExcelColumnsSelectionBehavior
    {
        private readonly IExcelCellValueExtractor _cellValueExtractor;

        /// <summary> .cctor </summary>
        /// <param name="cellValueExtractor">IExcelCellValueExtractor</param>
        public FirstRowIsColumnsColumnsSelectionBehavior(IExcelCellValueExtractor cellValueExtractor)
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
            var headerRow = rows.Take(1).Single();
            return ExtractColumns(headerRow, sharedStrings, propertyToColumnCaption);
        }

        private DataColumn[] ExtractColumns(
            Row row,
            IReadOnlyDictionary<int, string> sharedStrings,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            return row
                .Elements<Cell>()
                .Select((cell, index) => _cellValueExtractor.ExtractCellValue(cell, row.RowIndex, (uint)index, sharedStrings))
                .Where(cell => cell.Value != null
                               && propertyToColumnCaption.Values.Contains(cell.Value, StringComparer.Ordinal))
                .Select(cell => new DataColumn(cell.ColumnName)
                {
                    Caption = cell.Value
                })
                .ToArray();
        }
    }
}