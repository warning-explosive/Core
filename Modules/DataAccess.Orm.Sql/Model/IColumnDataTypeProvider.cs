namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;

    /// <summary>
    /// IColumnDataTypeProvider
    /// </summary>
    public interface IColumnDataTypeProvider
    {
        /// <summary>
        /// Gets column data type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Column data type</returns>
        string GetColumnDataType(Type type);
    }
}