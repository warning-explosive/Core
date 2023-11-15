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
        /// <param name="infos">Sheet infos</param>
        /// <returns>*.xlsx file represented as stream</returns>
        public MemoryStream ExportXlsx(ISheetInfo[] infos);
    }
}