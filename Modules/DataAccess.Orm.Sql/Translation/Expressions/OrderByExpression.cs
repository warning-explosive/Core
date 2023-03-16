namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// OrderByExpression
    /// </summary>
    public class OrderByExpression : ISqlExpression,
                                     IApplicable<OrderByExpression>,
                                     IApplicable<NamedSourceExpression>,
                                     IApplicable<QuerySourceExpression>,
                                     IApplicable<FilterExpression>,
                                     IApplicable<ProjectionExpression>,
                                     IApplicable<JoinExpression>,
                                     IApplicable<OrderByExpressionExpression>
    {
        private readonly List<ISqlExpression> _expressions;

        /// <summary> .cctor </summary>
        /// <param name="source">Source expression</param>
        /// <param name="expressions">Order by expressions</param>
        public OrderByExpression(
            ISqlExpression source,
            IReadOnlyCollection<ISqlExpression> expressions)
        {
            Source = source;
            _expressions = expressions.ToList();
        }

        internal OrderByExpression(Type type)
            : this(null!, Array.Empty<ISqlExpression>())
        {
        }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Order by expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions => _expressions;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, OrderByExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JoinExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, OrderByExpressionExpression expression)
        {
            ApplyExpression(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        private void ApplyExpression(ISqlExpression expression)
        {
            if (Source is JoinExpression join)
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

            _expressions.Add(expression);
        }

        #endregion
    }
}