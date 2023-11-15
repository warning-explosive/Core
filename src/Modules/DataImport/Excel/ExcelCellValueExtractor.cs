namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DocumentFormat.OpenXml.Spreadsheet;

    [Component(EnLifestyle.Singleton)]
    internal class ExcelCellValueExtractor : IExcelCellValueExtractor,
                                             IResolvable<IExcelCellValueExtractor>
    {
        private readonly IReadOnlyCollection<IRawCellValueVisitor> _cellValueVisitors;

        public ExcelCellValueExtractor(IEnumerable<IRawCellValueVisitor> cellValueVisitors)
        {
            _cellValueVisitors = cellValueVisitors.ToList();
        }

        public ExcelCellValue ExtractCellValue(
            Cell cell,
            uint rowIndex,
            uint columnIndex,
            IReadOnlyDictionary<int, string> sharedStrings)
        {
            var cellColumnName = cell.CellColumnName(rowIndex);

            string? value;

            switch (cell.DataType?.Value ?? CellValues.String)
            {
                case CellValues.InlineString:
                    value = cell.InnerText;
                    break;
                case CellValues.SharedString:
                    value = cell.CellValue != null
                        ? sharedStrings[int.Parse(cell.CellValue.Text, CultureInfo.InvariantCulture)]
                        : null;
                    break;
                default:
                    value = cell.CellValue?.Text;
                    break;
            }

            if (value != null)
            {
                value = _cellValueVisitors.Aggregate(value, (prev, visitor) => visitor.Visit(prev));
            }

            return new ExcelCellValue(rowIndex, columnIndex, cellColumnName, value);
        }
    }
}