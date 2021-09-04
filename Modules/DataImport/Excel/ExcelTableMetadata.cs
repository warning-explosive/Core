namespace SpaceEngineers.Core.DataImport.Excel
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;

    /// <summary>
    /// DataTable metadata
    /// </summary>
    public class ExcelTableMetadata : IDataTableMeta
    {
        /// <summary> .cctor </summary>
        public ExcelTableMetadata()
        {
            TableMetadata = new Dictionary<string, object>();
        }

        /// <summary> .cctor </summary>
        /// <param name="tableMetadata">Table metadata</param>
        public ExcelTableMetadata(IDictionary<string, object> tableMetadata)
        {
            TableMetadata = tableMetadata.ToDictionary(it => it.Key, it => it.Value);
        }

        /// <summary>
        /// Table metadata
        /// </summary>
        public IReadOnlyDictionary<string, object> TableMetadata { get; }
    }
}