namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// UnaryOperatorConversions
    /// </summary>
    public static class UnaryOperatorConversions
    {
        /// <summary>
        /// As ExpressionType
        /// </summary>
        /// <param name="operator">UnaryOperator</param>
        /// <returns>ExpressionType</returns>
        public static ExpressionType AsExpressionType(this UnaryOperator @operator)
        {
            return @operator switch
            {
                UnaryOperator.Not => ExpressionType.Not,
                _ => throw new NotSupportedException($"Not supported conversion from {nameof(UnaryOperator)}.{@operator} to {nameof(ExpressionType)}")
            };
        }

        /// <summary>
        /// As UnaryOperator
        /// </summary>
        /// <param name="type">ExpressionType</param>
        /// <returns>UnaryOperator</returns>
        public static UnaryOperator AsUnaryOperator(this ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Not => UnaryOperator.Not,
                _ => throw new NotSupportedException($"Not supported conversion from {nameof(ExpressionType)}.{type} to {nameof(UnaryOperator)}")
            };
        }
    }
}