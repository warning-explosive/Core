namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
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
        public IAsyncEnumerable<TElement> ExtractData(ExcelDataExtractorSpecification specification)
        {
            using (var stream = File.OpenRead(specification.FileInfo.FullName))
            using (var document = SpreadsheetDocument.Open(stream, false))
            {
                var worksheet = document
                    .WorkbookPart
                    .WorksheetParts
                    .SingleOrDefault(part => part.Uri.OriginalString.EndsWith(specification.SheetName + ".xml", StringComparison.Ordinal))
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

        private async IAsyncEnumerable<TElement> ProcessWorksheet(
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

                await foreach (var element in ReadTable(dataTable, propertyToColumn, specification.TableMetadata).ConfigureAwait(false))
                {
                    yield return element;
                }

                await _dataTableReader.AfterTableRead().ConfigureAwait(false);
            }
        }

        private DataRow ExtractDataRow(
            Row row,
            IReadOnlyDictionary<int, string> sharedStrings,
            DataTable dataTable)
        {
            var rowIndex = row.RowIndex.EnsureNotNull<uint>("Row should have index");

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

        private async IAsyncEnumerable<TElement> ReadTable(
            DataTable dataTable,
            IReadOnlyDictionary<string, string> propertyToColumn,
            ExcelTableMetadata tableMetadata)
        {
            for (var i = 0; i < dataTable.Rows.Count; ++i)
            {
                var row = dataTable.Rows[i];

                var element = await ExecutionExtensions
                    .TryAsync(() => _dataTableReader.ReadRow(row, i, propertyToColumn, tableMetadata))
                    .Catch<Exception>(ex => throw new InvalidOperationException($"Error in row {i}", ex))
                    .Invoke()
                    .ConfigureAwait(false);

                if (element != null)
                {
                    yield return element;
                }
            }
        }
    }
}