namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// ISqlExpression
    /// </summary>
    public interface ISqlExpression
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Converts ISqlExpression back to System.Expression
        /// </summary>
        /// <returns>Expression tree</returns>
        Expression AsExpressionTree();
    }
}