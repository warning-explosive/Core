namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using Basics.Enumerations;

    /// <summary>
    /// OrderByExpressionExpression
    /// </summary>
    public class OrderByExpressionExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="expression">Expression</param>
        /// <param name="orderingDirection">Ordering direction</param>
        public OrderByExpressionExpression(
            ISqlExpression expression,
            EnOrderingDirection orderingDirection)
        {
            if (expression is not BinaryExpression
                && expression is not ColumnsChainExpression
                && expression is not ColumnExpression
                && expression is not ConditionalExpression
                && expression is not JsonAttributeExpression
                && expression is not MethodCallExpression
                && expression is not NullExpression
                && expression is not ParameterExpression
                && expression is not ParenthesesExpression
                && expression is not QueryParameterExpression
                && expression is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(OrderByExpressionExpression)} doesn't support {expression.GetType().Name} as {nameof(expression)} argument");
            }

            Expression = expression;
            OrderingDirection = orderingDirection;
        }

        /// <summary>
        /// Expression
        /// </summary>
        public ISqlExpression Expression { get; }

        /// <summary>
        /// Ordering direction
        /// </summary>
        public EnOrderingDirection OrderingDirection { get; }
    }
}