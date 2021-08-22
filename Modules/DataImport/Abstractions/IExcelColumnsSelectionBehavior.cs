namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using System.Data;
    using AutoRegistration.Api.Abstractions;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Excel columns selection behavior
    /// </summary>
    public interface IExcelColumnsSelectionBehavior : IResolvable
    {
        /// <summary>
        /// First row is header
        /// </summary>
        bool FirstRowIsHeader { get; }

        /// <summary>
        /// Extract columns from rows
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="sharedStrings">Shared strings</param>
        /// <param name="propertyToColumnCaption">Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)</param>
        /// <returns>Columns</returns>
        DataColumn[] ExtractColumns(
            IEnumerable<Row> rows,
            IReadOnlyDictionary<int, string> sharedStrings,
            IReadOnlyDictionary<string, string> propertyToColumnCaption);
    }
}