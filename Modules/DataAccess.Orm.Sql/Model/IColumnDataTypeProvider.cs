namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    /// <summary>
    /// IColumnDataTypeProvider
    /// </summary>
    public interface IColumnDataTypeProvider
    {
        /// <summary>
        /// Gets column data type
        /// </summary>
        /// <param name="columnInfo">ColumnInfo</param>
        /// <returns>Column data type</returns>
        string GetColumnDataType(ColumnInfo columnInfo);
    }
}