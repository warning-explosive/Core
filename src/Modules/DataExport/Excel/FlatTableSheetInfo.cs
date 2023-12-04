namespace SpaceEngineers.Core.DataExport.Excel
{
    using System.Collections.Generic;

    /// <summary>
    /// FlatTableSheetInfo
    /// </summary>
    /// <typeparam name="TRow">TRow type-argument</typeparam>
    public sealed class FlatTableSheetInfo<TRow> : ISheetInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="sheetName">sheetName</param>
        /// <param name="flatTable">flatTable</param>
        public FlatTableSheetInfo(string sheetName, IReadOnlyCollection<TRow> flatTable)
        {
            SheetName = sheetName;
            FlatTable = flatTable;
        }

        /// <summary>
        /// Sheet name
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Flat table
        /// </summary>
        public IReadOnlyCollection<TRow> FlatTable { get; }
    }
}