namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Excel;

    /// <summary>
    /// Excel cell value extractor
    /// </summary>
    public interface IExcelCellValueExtractor
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