namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// IIntermediateExpression
    /// </summary>
    public interface IIntermediateExpression
    {
        /// <summary>
        /// Type
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Converts IIntermediateExpression back to expression tree
        /// </summary>
        /// <returns>Expression tree</returns>
        Expression AsExpressionTree();
    }
}