namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IColumnDataTypeProvider
    /// </summary>
    public interface IColumnDataTypeProvider : IResolvable
    {
        /// <summary>
        /// Gets column data type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Column data type</returns>
        string GetColumnDataType(Type type);
    }
}