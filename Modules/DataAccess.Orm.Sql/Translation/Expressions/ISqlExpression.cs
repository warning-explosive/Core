namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ISqlExpression
    /// </summary>
    public interface ISqlExpression
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }
    }
}