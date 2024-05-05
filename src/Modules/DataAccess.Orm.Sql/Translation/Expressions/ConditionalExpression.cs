namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ConditionalExpression
    /// </summary>
    public class ConditionalExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="when">When expression</param>
        /// <param name="then">Then expression</param>
        /// <param name="else">Else expression</param>
        public ConditionalExpression(
            Type type,
            ISqlExpression when,
            ISqlExpression then,
            ISqlExpression @else)
        {
            if (when is not BinaryExpression
                && when is not ColumnsChainExpression
                && when is not ColumnExpression
                && when is not ConditionalExpression
                && when is not JsonAttributeExpression
                && when is not MethodCallExpression
                && when is not NullExpression
                && when is not ParameterExpression
                && when is not ParenthesesExpression
                && when is not QueryParameterExpression
                && when is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(ConditionalExpression)} doesn't support {when.GetType().Name} as {nameof(when)} argument");
            }

            if (then is not BinaryExpression
                && then is not ColumnsChainExpression
                && then is not ColumnExpression
                && then is not ConditionalExpression
                && then is not JsonAttributeExpression
                && then is not MethodCallExpression
                && then is not NullExpression
                && then is not ParameterExpression
                && then is not ParenthesesExpression
                && then is not QueryParameterExpression
                && then is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(ConditionalExpression)} doesn't support {then.GetType().Name} as {nameof(then)} argument");
            }

            if (@else is not BinaryExpression
                && @else is not ColumnsChainExpression
                && @else is not ColumnExpression
                && @else is not ConditionalExpression
                && @else is not JsonAttributeExpression
                && @else is not MethodCallExpression
                && @else is not NullExpression
                && @else is not ParameterExpression
                && @else is not ParenthesesExpression
                && @else is not QueryParameterExpression
                && @else is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(ConditionalExpression)} doesn't support {@else.GetType().Name} as {nameof(@else)} argument");
            }

            Type = type;
            When = when;
            Then = then;
            Else = @else;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// When condition
        /// </summary>
        public ISqlExpression When { get; }

        /// <summary>
        /// Then expression
        /// </summary>
        public ISqlExpression Then { get; }

        /// <summary>
        /// Then expression
        /// </summary>
        public ISqlExpression Else { get; }
    }
}