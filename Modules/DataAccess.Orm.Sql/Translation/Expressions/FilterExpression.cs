namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// FilterExpression
    /// </summary>
    public class FilterExpression : ISqlExpression,
                                    IApplicable<DeleteExpression>,
                                    IApplicable<SetExpression>,
                                    IApplicable<ProjectionExpression>,
                                    IApplicable<JoinExpression>,
                                    IApplicable<QuerySourceExpression>,
                                    IApplicable<QueryParameterExpression>,
                                    IApplicable<ParameterExpression>,
                                    IApplicable<ParenthesesExpression>,
                                    IApplicable<BinaryExpression>,
                                    IApplicable<UnaryExpression>,
                                    IApplicable<ConditionalExpression>,
                                    IApplicable<ColumnExpression>,
                                    IApplicable<JsonAttributeExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source expression</param>
        /// <param name="predicate">Predicate expression</param>
        public FilterExpression(
            ISqlExpression source,
            ISqlExpression predicate)
        {
            Source = source;
            Predicate = predicate;
        }

        internal FilterExpression()
            : this(null!, null!)
        {
        }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Predicate expression
        /// </summary>
        public ISqlExpression Predicate { get; private set; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParenthesesExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, UnaryExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JsonAttributeExpression expression)
        {
            ApplyPredicate(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, DeleteExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, SetExpression expression)
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
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        private void ApplyPredicate(ISqlExpression expression)
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

            Predicate = Predicate != null
                ? new BinaryExpression(typeof(bool), BinaryOperator.AndAlso, Predicate, expression)
                : expression;
        }

        #endregion
    }
}