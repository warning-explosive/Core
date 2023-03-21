namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Linq
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// ExecutedExpressionEventArgs
    /// </summary>
    public class ExecutedExpressionEventArgs : EventArgs
    {
        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        public ExecutedExpressionEventArgs(Expression expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Expression
        /// </summary>
        public Expression Expression { get; }
    }
}