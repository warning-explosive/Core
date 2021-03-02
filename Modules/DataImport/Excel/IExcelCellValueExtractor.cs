namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Excel cell value extractor
    /// </summary>
    public interface IExcelCellValueExtractor : IResolvable
    {
        /// <summary>
        /// Extract excel cell value
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnIndex">Column index</param>
        /// <param name="sharedStrings">Shared strings</param>
        /// <returns>ExcelCellValue</returns>
        ExcelCellValue ExtractCellValue(
            Cell cell,
            uint rowIndex,
            uint columnIndex,
            IReadOnlyDictionary<int, string> sharedStrings);
    }
}