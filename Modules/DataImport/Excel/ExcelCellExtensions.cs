namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Globalization;
    using DocumentFormat.OpenXml.Spreadsheet;

    internal static class ExcelCellExtensions
    {
        internal static string CellColumnName(this Cell cell, uint rowIndex)
        {
            var reference = cell.CellReference.Value;
            return reference.Substring(0, reference.Length - rowIndex.ToString(CultureInfo.InvariantCulture).Length);
        }
    }
}