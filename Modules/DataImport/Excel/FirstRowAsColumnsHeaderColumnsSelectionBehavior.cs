namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using DocumentFormat.OpenXml.Spreadsheet;

    [Component(EnLifestyle.Singleton)]
    internal class FirstRowAsColumnsHeaderColumnsSelectionBehavior : IExcelColumnsSelectionBehavior,
                                                                     IResolvable<IExcelColumnsSelectionBehavior>
    {
        private readonly IExcelCellValueExtractor _cellValueExtractor;

        public FirstRowAsColumnsHeaderColumnsSelectionBehavior(IExcelCellValueExtractor cellValueExtractor)
        {
            _cellValueExtractor = cellValueExtractor;
        }

        public bool FirstRowIsHeader { get; } = true;

        public DataColumn[] ExtractColumns(
            IEnumerable<Row> rows,
            IReadOnlyDictionary<int, string> sharedStrings,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            var headerRow = rows.Take(1).Single();

            var headerRowIndex = (headerRow.RowIndex?.Value).EnsureNotNull<uint>("Row should have index");

            return headerRow
                .Elements<Cell>()
                .Select((cell, index) => _cellValueExtractor.ExtractCellValue(cell, headerRowIndex, (uint)index, sharedStrings))
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