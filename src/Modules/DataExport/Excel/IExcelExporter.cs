namespace SpaceEngineers.Core.DataExport.Excel
{
    using System.IO;

    /// <summary>
    /// IExcelExporter
    /// </summary>
    public interface IExcelExporter
    {
        /// <summary>
        /// Export datasets to *.xlsx file
        /// </summary>
        /// <param name="sheets">Sheet infos</param>
        /// <returns>*.xlsx file represented as stream</returns>
        public Stream ExportXlsx(ISheetInfo[] sheets);
    }
}