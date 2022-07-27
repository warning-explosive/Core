namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Abstractions;
    using Basics;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <inheritdoc />
    public abstract class ExcelDataExtractor<TElement> : IDataExtractor<TElement, ExcelDataExtractorSpecification>
    {
        private readonly IExcelCellValueExtractor _cellValueExtractor;
        private readonly IExcelColumnsSelectionBehavior _columnsSelectionBehavior;
        private readonly IDataTableReader<TElement, ExcelTableMetadata> _dataTableReader;

        /// <summary> .cctor </summary>
        /// <param name="cellValueExtractor">IExcelCellValueExtractor</param>
        /// <param name="columnsSelectionBehavior">IExcelColumnsSelectionBehavior</param>
        /// <param name="dataTableReader">IDataTableReader</param>
        protected ExcelDataExtractor(
            IExcelCellValueExtractor cellValueExtractor,
            IExcelColumnsSelectionBehavior columnsSelectionBehavior,
            IDataTableReader<TElement, ExcelTableMetadata> dataTableReader)
        {
            _cellValueExtractor = cellValueExtractor;
            _columnsSelectionBehavior = columnsSelectionBehavior;
            _dataTableReader = dataTableReader;
        }

        /// <inheritdoc />
        public IEnumerable<TElement> ExtractData(ExcelDataExtractorSpecification specification)
        {
            using (var document = SpreadsheetDocument.Open(specification.File, false))
            {
                var sheet = document
                   .WorkbookPart
                   .Workbook
                   .Sheets
                   .EnsureNotNull("Workbook has no sheets")
                   .OfType<Sheet>()
                   .SingleOrDefault(sheet => sheet.Name.Value.Equals(specification.SheetName, StringComparison.Ordinal))
                   .EnsureNotNull($"Worksheet {specification.SheetName} not found");

                var worksheet = document
                   .WorkbookPart
                   .WorksheetParts
                   .SingleOrDefault(part => part.Uri.OriginalString.EndsWith(sheet.LocalName + sheet.SheetId + ".xml", StringComparison.Ordinal))
                   .EnsureNotNull($"Worksheet {specification.SheetName} not found")
                   .Worksheet;

                var sharedStrings = SharedStrings(document);

                return ProcessWorksheet(worksheet, sharedStrings, specification);
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

        private IEnumerable<TElement> ProcessWorksheet(
            Worksheet worksheet,
            IReadOnlyDictionary<int, string> sharedStrings,
            ExcelDataExtractorSpecification specification)
        {
            using (var dataTable = new DataTable())
            {
                foreach (var sheetData in worksheet.Elements<SheetData>())
                {
                    var rows = sheetData.Elements<Row>();

                    if (!specification.Range.Equals(Range.All))
                    {
                        rows = rows.Where(row => (row.RowIndex.Value - 1).BetweenInclude(specification.Range));
                    }

                    var columns = _columnsSelectionBehavior
                        .ExtractColumns(rows, sharedStrings, _dataTableReader.PropertyToColumnCaption);

                    dataTable.Columns.AddRange(columns);

                    var dataRows = _columnsSelectionBehavior.FirstRowIsHeader
                        ? rows.Skip(1)
                        : rows;

                    foreach (var dataRow in dataRows)
                    {
                        var row = ExtractDataRow(dataRow, sharedStrings, dataTable);
                        dataTable.Rows.Add(row);
                    }
                }

                var propertyToColumn = MergeColumns(dataTable);

                foreach (var element in ReadTable(dataTable, propertyToColumn, specification.TableMetadata))
                {
                    yield return element;
                }

                _dataTableReader.AfterTableRead();
            }
        }

        private DataRow ExtractDataRow(
            Row row,
            IReadOnlyDictionary<int, string> sharedStrings,
            DataTable dataTable)
        {
            var rowIndex = (row.RowIndex?.Value).EnsureNotNull<uint>("Row should have index");

            return row
                .Elements<Cell>()
                .Where(cell => dataTable.Columns.Contains(cell.CellColumnName(rowIndex)))
                .Select((cell, index) => _cellValueExtractor.ExtractCellValue(cell, rowIndex, (uint)index, sharedStrings))
                .Aggregate(dataTable.NewRow(), SetValue);

            static DataRow SetValue(DataRow row, ExcelCellValue cell)
            {
                row[cell.ColumnName] = cell.Value;
                return row;
            }
        }

        private IReadOnlyDictionary<string, string> MergeColumns(DataTable dataTable)
        {
            return _dataTableReader.PropertyToColumnCaption
                .Join(dataTable.Columns.OfType<DataColumn>(),
                    col => col.Value,
                    col => col.Caption,
                    (p, dc) =>
                    {
                        var propertyName = p.Key;
                        var dataColumnName = dc.ColumnName;
                        return (propertyName, dataColumnName);
                    })
                .ToDictionary(column => column.propertyName,
                    column => column.dataColumnName);
        }

        private IEnumerable<TElement> ReadTable(
            DataTable dataTable,
            IReadOnlyDictionary<string, string> propertyToColumn,
            ExcelTableMetadata tableMetadata)
        {
            for (var i = 0; i < dataTable.Rows.Count; ++i)
            {
                var row = dataTable.Rows[i];

                var element = ExecutionExtensions
                    .Try((row, i, propertyToColumn, tableMetadata), ReadRow(_dataTableReader))
                    .Catch<Exception>()
                    .Invoke(RowError(i));

                if (element != null)
                {
                    yield return element;
                }
            }

            static Func<(DataRow, int, IReadOnlyDictionary<string, string>, ExcelTableMetadata), TElement?> ReadRow(
                IDataTableReader<TElement, ExcelTableMetadata> dataTableReader)
            {
                return state =>
                {
                    var (row, i, propertyToColumn, tableMetadata) = state;
                    return dataTableReader.ReadRow(row, i, propertyToColumn, tableMetadata);
                };
            }

            static Func<Exception, TElement?> RowError(int i)
            {
                return exception => throw new InvalidOperationException($"Error in row {i}", exception);
            }
        }
    }
}