namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using AutoWiring.Api.Attributes;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// First row is columns header columns selection behavior
    /// </summary>
    [Unregistered]
    public class FirstRowIsColumnsHeaderColumnsSelectionBehavior : IExcelColumnsSelectionBehavior
    {
        private readonly IExcelCellValueExtractor _cellValueExtractor;

        /// <summary> .cctor </summary>
        /// <param name="cellValueExtractor">IExcelCellValueExtractor</param>
        public FirstRowIsColumnsHeaderColumnsSelectionBehavior(IExcelCellValueExtractor cellValueExtractor)
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

            return headerRow
                .Elements<Cell>()
                .Select((cell, index) => _cellValueExtractor.ExtractCellValue(cell, headerRow.RowIndex, (uint)index, sharedStrings))
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