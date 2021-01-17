namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using Abstractions;

    /// <summary>
    /// DataTable metadata
    /// </summary>
    public class ExcelTableMetadata : IDataTableMeta
    {
        /// <summary> .cctor </summary>
        public ExcelTableMetadata()
        {
            TableMetadata = new Dictionary<string, string>();
        }

        /// <summary> .cctor </summary>
        /// <param name="tableMetadata">Table metadata</param>
        public ExcelTableMetadata(IReadOnlyDictionary<string, string> tableMetadata)
        {
            TableMetadata = tableMetadata;
        }

        /// <summary>
        /// Table metadata
        /// </summary>
        private IReadOnlyDictionary<string, string> TableMetadata { get; }
    }
}