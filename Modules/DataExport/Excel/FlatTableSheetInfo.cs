namespace SpaceEngineers.Core.DataExport.Excel
{
    using System.Collections.Generic;

    /// <summary>
    /// FlatTableSheetInfo
    /// </summary>
    /// <typeparam name="TRow">TRow type-argument</typeparam>
    public class FlatTableSheetInfo<TRow> : BaseSheetInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="flatTable">Flat table</param>
        public FlatTableSheetInfo(IReadOnlyCollection<TRow> flatTable)
        {
            FlatTable = flatTable;
        }

        /// <summary>
        /// Flat table
        /// </summary>
        public IReadOnlyCollection<TRow> FlatTable { get; }
    }
}