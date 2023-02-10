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
        /// <param name="column">ColumnInfo</param>
        /// <returns>Column data type</returns>
        string GetColumnDataType(ColumnInfo column);
    }
}