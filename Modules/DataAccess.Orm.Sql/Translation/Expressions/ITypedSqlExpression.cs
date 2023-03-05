namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ITypedSqlExpression
    /// </summary>
    public interface ITypedSqlExpression : ISqlExpression
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }
    }
}