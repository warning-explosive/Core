namespace SpaceEngineers.Core.DataImport.Abstractions
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Data table reader
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    /// <typeparam name="TTableMeta">TTableMeta type-argument</typeparam>
    public interface IDataTableReader<TElement, TTableMeta>
        where TTableMeta : IDataTableMeta
    {
        /// <summary>
        /// Property to column caption map (PropertyInfo.Name -> DataTable.ColumnCaption)
        /// </summary>
        IReadOnlyDictionary<string, string> PropertyToColumnCaption { get; }

        /// <summary>
        /// Read data row
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="rowIndex">Row index (relative)</param>
        /// <param name="propertyToColumn">Property to column map (PropertyInfo.Name -> DataTable.ColumnId)</param>
        /// <param name="tableMeta">DataTable meta</param>
        /// <returns>Element or null if row isn't valid</returns>
        Task<TElement?> ReadRow(
            DataRow row,
            int rowIndex,
            IReadOnlyDictionary<string, string> propertyToColumn,
            TTableMeta tableMeta);

        /// <summary>
        /// After table read
        /// </summary>
        /// <returns>Ongoing operation</returns>
        Task AfterTableRead();
    }
}