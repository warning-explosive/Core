namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Enumerations;
    using Basics;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class ExcelDataExtractor : IDataExtractor<ExcelDataExtractorSpecification>
    {
        private readonly IReadOnlyCollection<IRawCellValueVisitor> _cellValueVisitors;

        /// <summary> .cctor </summary>
        /// <param name="cellValueVisitors">Cell value visitors</param>
        public ExcelDataExtractor(IEnumerable<IRawCellValueVisitor> cellValueVisitors)
        {
            _cellValueVisitors = cellValueVisitors.ToList();
        }

        /// <inheritdoc />
        public DataTable ExtractData(
            ExcelDataExtractorSpecification specification,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            using (var document = SpreadsheetDocument.Open(specification.DataStream, false))
            {
                var worksheet = document
                    .WorkbookPart
                    .WorksheetParts
                    .SingleOrDefault(part => part.Uri.OriginalString.EndsWith(specification.SheetName + ".xml", StringComparison.Ordinal))
                    .EnsureNotNull($"Worksheet {specification.SheetName} not found")
                    .Worksheet;

                var sharedStrings = SharedStrings(document);

                return ProcessWorksheet(worksheet, sharedStrings, specification, propertyToColumnCaption);
            }
        }

        private static IReadOnlyDictionary<int, string> SharedStrings(SpreadsheetDocument document)
        {
            return document
                       .WorkbookPart
                       .SharedStringTablePart
                      ?.SharedStringTable
                       .Elements<SharedStringItem>()
                       .Select((s, index) => (s.InnerText, index))
                       .ToDictionary(pair => pair.index, pair => pair.InnerText)
                   ?? new Dictionary<int, string>();
        }

        private DataTable ProcessWorksheet(
            Worksheet worksheet,
            IReadOnlyDictionary<int, string> sharedStrings,
            ExcelDataExtractorSpecification specification,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            var dataTable = new DataTable();

            foreach (var sheetData in worksheet.Elements<SheetData>())
            {
                var rows = sheetData
                        .Elements<Row>()
                        .Where(row => row.RowIndex.Value.BetweenInclude(specification.Range));

                var headerRow = rows.Take(1).Single();
                var columns = ExtractColumns(headerRow, sharedStrings, propertyToColumnCaption);
                dataTable.Columns.AddRange(columns);

                foreach (var dataRow in rows.Skip(1))
                {
                    var row = ExtractDataRow(dataRow, sharedStrings, dataTable);
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        private DataRow ExtractDataRow(
            Row row,
            IReadOnlyDictionary<int, string> sharedStrings,
            DataTable dataTable)
        {
            return row
                .Elements<Cell>()
                .Where(cell => dataTable.Columns.Contains(CellColumnName(cell, row.RowIndex)))
                .Select((cell, index) => ExtractCellValue(cell, row.RowIndex, (uint)index, sharedStrings))
                .Aggregate(dataTable.NewRow(),
                    (row, cell) =>
                    {
                        row[cell.ColumnName] = cell.Value;
                        return row;
                    });
        }

        private DataColumn[] ExtractColumns(
            Row row,
            IReadOnlyDictionary<int, string> sharedStrings,
            IReadOnlyDictionary<string, string> propertyToColumnCaption)
        {
            return row
                .Elements<Cell>()
                .Select((cell, index) => ExtractCellValue(cell, row.RowIndex, (uint)index, sharedStrings))
                .Where(cell => cell.Value != null
                               && propertyToColumnCaption.Values.Contains(cell.Value, StringComparer.Ordinal))
                .Select(cell => new DataColumn(cell.ColumnName)
                {
                    Caption = cell.Value
                })
                .ToArray();
        }

        private ExcelCellValue ExtractCellValue(
            Cell cell,
            uint rowIndex,
            uint columnIndex,
            IReadOnlyDictionary<int, string> sharedStrings)
        {
            var cellColumnName = CellColumnName(cell, rowIndex);

            string? value = null;

            if (cell.DataType.HasValue)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.InlineString:
                        value = cell.InnerText;
                        break;
                    case CellValues.SharedString:
                        value = cell.CellValue != null
                            ? sharedStrings[int.Parse(cell.CellValue.Text)]
                            : null;
                        break;
                    default:
                        value = cell.CellValue?.Text;
                        break;
                }
            }

            if (value == null)
            {
                return new ExcelCellValue(rowIndex, columnIndex, cellColumnName, null);
            }

            value = _cellValueVisitors.Aggregate(value, (prev, visitor) => visitor.Visit(prev));

            return new ExcelCellValue(rowIndex, columnIndex, cellColumnName, value);
        }

        private static string CellColumnName(Cell cell, uint rowIndex)
        {
            var reference = cell.CellReference.Value;
            return reference.Substring(0, reference.Length - rowIndex.ToString().Length);
        }
    }
}