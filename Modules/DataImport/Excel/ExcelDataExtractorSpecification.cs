namespace SpaceEngineers.Core.DataImport.Excel
{
    using System;
    using System.IO;
    using Abstractions;

    /// <summary>
    /// Excel data extractor specification
    /// </summary>
    public class ExcelDataExtractorSpecification : IDataExtractorSpecification
    {
        /// <summary> .cctor </summary>
        /// <param name="file">Stream</param>
        /// <param name="sheetName">Sheet name</param>
        /// <param name="range">Rows range for import</param>
        /// <param name="tableMetadata">Excel table metadata</param>
        public ExcelDataExtractorSpecification(
            Stream file,
            string sheetName,
            Range range,
            ExcelTableMetadata tableMetadata)
        {
            File = file;
            SheetName = sheetName;
            Range = range;
            TableMetadata = tableMetadata;
        }

        /// <summary>
        /// File info
        /// </summary>
        public Stream File { get; }

        /// <summary>
        /// Sheet name
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Rows range for import
        /// </summary>
        public Range Range { get; }

        /// <summary>
        /// DataTable metadata
        /// </summary>
        public ExcelTableMetadata TableMetadata { get; }
    }
}