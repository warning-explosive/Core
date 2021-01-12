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
        /// <param name="dataStream">Data stream</param>
        /// <param name="sheetName">Sheet name</param>
        /// <param name="range">Rows range for import</param>
        public ExcelDataExtractorSpecification(
            Stream dataStream,
            string sheetName,
            Range range)
        {
            DataStream = dataStream;
            SheetName = sheetName;
            Range = range;
        }

        /// <summary>
        /// Data stream
        /// </summary>
        public Stream DataStream { get; }

        /// <summary>
        /// Sheet name
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Rows range for import
        /// </summary>
        public Range Range { get; }
    }
}