namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// BinaryExpression
    /// </summary>
    public class BinaryExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="left">Left expression</param>
        /// <param name="right">Right expression</param>
        public BinaryExpression(
            Type type,
            BinaryOperator @operator,
            ISqlExpression left,
            ISqlExpression right)
        {
            if (left is not BinaryExpression
                && left is not ColumnExpression
                && left is not ConditionalExpression
                && left is not JsonAttributeExpression
                && left is not MethodCallExpression
                && left is not NullExpression
                && left is not ParameterExpression
                && left is not ParenthesesExpression
                && left is not QueryParameterExpression
                && left is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(BinaryExpression)} doesn't support {left.GetType().Name} as {nameof(left)} argument");
            }

            if (right is not BinaryExpression
                && right is not ColumnExpression
                && right is not ConditionalExpression
                && right is not JsonAttributeExpression
                && right is not MethodCallExpression
                && right is not NullExpression
                && right is not ParameterExpression
                && right is not ParenthesesExpression
                && right is not QueryParameterExpression
                && right is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(BinaryExpression)} doesn't support {right.GetType().Name} as {nameof(right)} argument");
            }

            Type = type;
            Operator = @operator;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public BinaryOperator Operator { get; }

        /// <summary>
        /// Left expression
        /// </summary>
        public ISqlExpression Left { get; }

        /// <summary>
        /// Right expression
        /// </summary>
        public ISqlExpression Right { get; }
    }
}