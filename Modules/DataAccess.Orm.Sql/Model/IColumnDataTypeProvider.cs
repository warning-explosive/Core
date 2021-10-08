namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    using System;
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using Orm.Model;

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

        /// <summary>
        /// Gets column type
        /// </summary>
        /// <param name="dataType">Column data type</param>
        /// <returns>Column type</returns>
        Type GetColumnType(string dataType);

        /// <summary>
        /// Gets column modifiers
        /// </summary>
        /// <param name="createColumn">CreateColumn</param>
        /// <returns>Column modifiers</returns>
        IEnumerable<string> GetModifiers(CreateColumn createColumn);
    }
}