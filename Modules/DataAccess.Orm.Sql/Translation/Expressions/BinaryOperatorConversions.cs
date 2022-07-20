namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// BinaryOperatorConversions
    /// </summary>
    public static class BinaryOperatorConversions
    {
        /// <summary>
        /// As ExpressionType
        /// </summary>
        /// <param name="operator">BinaryOperator</param>
        /// <returns>ExpressionType</returns>
        public static ExpressionType AsExpressionType(this BinaryOperator @operator)
        {
            return @operator switch
            {
                BinaryOperator.Equal => ExpressionType.Equal,
                BinaryOperator.NotEqual => ExpressionType.NotEqual,
                BinaryOperator.GreaterThanOrEqual => ExpressionType.GreaterThanOrEqual,
                BinaryOperator.GreaterThan => ExpressionType.GreaterThan,
                BinaryOperator.LessThan => ExpressionType.LessThan,
                BinaryOperator.LessThanOrEqual => ExpressionType.LessThanOrEqual,
                BinaryOperator.AndAlso => ExpressionType.AndAlso,
                BinaryOperator.OrElse => ExpressionType.OrElse,
                BinaryOperator.ExclusiveOr => ExpressionType.ExclusiveOr,
                BinaryOperator.Coalesce => ExpressionType.Coalesce,
                _ => throw new NotSupportedException($"Not supported conversion from {nameof(BinaryOperator)}.{@operator} to {nameof(ExpressionType)}")
            };
        }

        /// <summary>
        /// As BinaryOperator
        /// </summary>
        /// <param name="type">ExpressionType</param>
        /// <returns>BinaryOperator</returns>
        public static BinaryOperator AsBinaryOperator(this ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => BinaryOperator.Equal,
                ExpressionType.NotEqual => BinaryOperator.NotEqual,
                ExpressionType.GreaterThanOrEqual => BinaryOperator.GreaterThanOrEqual,
                ExpressionType.GreaterThan => BinaryOperator.GreaterThan,
                ExpressionType.LessThan => BinaryOperator.LessThan,
                ExpressionType.LessThanOrEqual => BinaryOperator.LessThanOrEqual,
                ExpressionType.AndAlso => BinaryOperator.AndAlso,
                ExpressionType.OrElse => BinaryOperator.OrElse,
                ExpressionType.ExclusiveOr => BinaryOperator.ExclusiveOr,
                ExpressionType.Coalesce => BinaryOperator.Coalesce,
                ExpressionType.Add => BinaryOperator.Add,
                ExpressionType.Subtract => BinaryOperator.Subtract,
                ExpressionType.Divide => BinaryOperator.Divide,
                ExpressionType.Multiply => BinaryOperator.Multiply,
                ExpressionType.Modulo => BinaryOperator.Modulo,
                _ => throw new NotSupportedException($"Not supported conversion from {nameof(ExpressionType)}.{type} to {nameof(BinaryOperator)}")
            };
        }
    }
}