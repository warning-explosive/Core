namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// UnaryExpression
    /// </summary>
    public class UnaryExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="operator">Operator</param>
        /// <param name="source">Source</param>
        public UnaryExpression(
            Type type,
            UnaryOperator @operator,
            ISqlExpression source)
        {
            if (source is not BinaryExpression
                && source is not ColumnsChainExpression
                && source is not ColumnExpression
                && source is not ConditionalExpression
                && source is not JsonAttributeExpression
                && source is not MethodCallExpression
                && source is not NullExpression
                && source is not ParameterExpression
                && source is not ParenthesesExpression
                && source is not QueryParameterExpression
                && source is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(UnaryExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Type = type;
            Operator = @operator;
            Source = source;
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Binary operator
        /// </summary>
        public UnaryOperator Operator { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }
    }
}