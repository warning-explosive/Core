namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// FilterExpression
    /// </summary>
    public class FilterExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="itemType">ItemType</param>
        /// <param name="predicate">Predicate expression</param>
        public FilterExpression(
            Type itemType,
            ISqlExpression predicate)
        {
            if (predicate is not BinaryExpression
                && predicate is not ColumnsChainExpression
                && predicate is not ColumnExpression
                && predicate is not ConditionalExpression
                && predicate is not JsonAttributeExpression
                && predicate is not MethodCallExpression
                && predicate is not NullExpression
                && predicate is not ParameterExpression
                && predicate is not ParenthesesExpression
                && predicate is not QueryParameterExpression
                && predicate is not UnaryExpression)
            {
                throw new ArgumentException($"{nameof(FilterExpression)} doesn't support {predicate.GetType().Name} as {nameof(predicate)} argument");
            }

            ItemType = itemType;
            Predicate = predicate;
        }

        /// <summary>
        /// ItemType
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Predicate expression
        /// </summary>
        public ISqlExpression Predicate { get; }
    }
}