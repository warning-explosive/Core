namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// OrderByExpression
    /// </summary>
    public class OrderByExpression : ISqlExpression
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source expression</param>
        /// <param name="expressions">Order by expressions</param>
        public OrderByExpression(
            ISqlExpression source,
            IReadOnlyCollection<OrderByExpressionExpression> expressions)
        {
            if (source is not FilterExpression
                && source is not JoinExpression
                && source is not NamedSourceExpression
                && source is not ProjectionExpression)
            {
                throw new ArgumentException($"{nameof(OrderByExpression)} doesn't support {source.GetType().Name} as {nameof(source)} argument");
            }

            Source = source;
            Expressions = expressions.ToList();

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

            _expressions.Add(expression);*/
        }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; }

        /// <summary>
        /// Order by expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions { get; }
    }
}