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
        /// <param name="source">Source expression</param>
        /// <param name="predicate">Predicate expression</param>
        public FilterExpression(
            Type itemType,
            ISqlExpression source,
            ISqlExpression predicate)
        {
            if (source is not DeleteExpression
                && source is not ProjectionExpression
                && source is not SetExpression)
            {
                throw new ArgumentException($"{nameof(FilterExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            if (predicate is not BinaryExpression
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
            Source = source;
            Predicate = predicate;

            // TODO: simplify to assigment
            /*if (Source is JoinExpression join)
            {
                expression = ReplaceJoinParameterExpressionsVisitor.Replace(expression, join);
            }
            else if (Source is ProjectionExpression projection)
            {
                expression = ReplaceProjectionExpressionVisitor.Compact(expression, projection);

                if (projection.Source is JoinExpression projectionJoin)
                {
                    expression = ReplaceJoinParameterExpressionsVisitor.Replace(expression, projectionJoin);
                }
            }

            Predicate = Predicate != null
                ? new BinaryExpression(typeof(bool), BinaryOperator.AndAlso, Predicate, expression)
                : expression;*/
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type ItemType { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Predicate expression
        /// </summary>
        public ISqlExpression Predicate { get; }
    }
}