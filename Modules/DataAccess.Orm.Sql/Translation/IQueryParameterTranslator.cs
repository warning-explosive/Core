﻿namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    /// <summary>
    /// IQueryParameterTranslator
    /// </summary>
    /// <typeparam name="TValue">TValue type-argument</typeparam>
    public interface IQueryParameterTranslator<in TValue>
    {
        /// <summary>
        /// Translates SQL query parameter value into sql expression
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Sql expression</returns>
        string Translate(TValue value); // TODO: #159 - use ISqlExpression instead of string
    }
}