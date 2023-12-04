namespace SpaceEngineers.Core.DataExport.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// PivotTableSheetInfo
    /// </summary>
    /// <typeparam name="TRow">TRow type-argument</typeparam>
    public class PivotTableSheetInfo<TRow> : ISheetInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="sheetName">sheetName</param>
        /// <param name="flatTable">flatTable</param>
        /// <param name="columnKey">columnKey</param>
        /// <param name="subGroupInfo">subGroupInfo</param>
        /// <param name="aggregateFunc">aggregateFunc</param>
        /// <param name="showAbsoluteNumbers">showAbsoluteNumbers</param>
        public PivotTableSheetInfo(
            string sheetName,
            IReadOnlyCollection<TRow> flatTable,
            Func<TRow, string> columnKey,
            SubGroupInfo<TRow> subGroupInfo,
            Func<IEnumerable<TRow>, decimal> aggregateFunc,
            bool showAbsoluteNumbers)
            : this(sheetName,
                flatTable,
                columnKey,
                new[] { subGroupInfo },
                aggregateFunc,
                showAbsoluteNumbers)
        {
        }

        /// <summary> .cctor </summary>
        /// <param name="sheetName">sheetName</param>
        /// <param name="flatTable">flatTable</param>
        /// <param name="columnKey">columnKey</param>
        /// <param name="subGroups">subGroups</param>
        /// <param name="aggregateFunc">aggregateFunc</param>
        /// <param name="showAbsoluteNumbers">showAbsoluteNumbers</param>
        public PivotTableSheetInfo(
            string sheetName,
            IReadOnlyCollection<TRow> flatTable,
            Func<TRow, string> columnKey,
            IEnumerable<SubGroupInfo<TRow>> subGroups,
            Func<IEnumerable<TRow>, decimal> aggregateFunc,
            bool showAbsoluteNumbers)
        {
            SheetName = sheetName;
            FlatTable = flatTable;
            ColumnKey = columnKey;
            SubGroups = subGroups.ToDictionary(it => it.Name, StringComparer.Ordinal);
            AggregateFunc = aggregateFunc;
            ShowAbsoluteNumbers = showAbsoluteNumbers;
        }

        /// <summary>
        /// Sheet name
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Flat table
        /// </summary>
        public IReadOnlyCollection<TRow> FlatTable { get; }

        /// <summary>
        /// Column key
        /// </summary>
        public Func<TRow, string> ColumnKey { get; }

        /// <summary>
        /// Subgroups
        /// </summary>
        public IReadOnlyDictionary<string, SubGroupInfo<TRow>> SubGroups { get; }

        /// <summary>
        /// Aggregate func
        /// </summary>
        public Func<IEnumerable<TRow>, decimal> AggregateFunc { get; }

        /// <summary>
        /// Show absolute numbers
        /// </summary>
        public bool ShowAbsoluteNumbers { get; }
    }
}