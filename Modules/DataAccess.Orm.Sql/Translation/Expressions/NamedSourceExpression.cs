namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// NamedSourceExpression
    /// </summary>
    public class NamedSourceExpression : ISqlExpression,
                                         IApplicable<FilterExpression>,
                                         IApplicable<ProjectionExpression>,
                                         IApplicable<OrderByExpression>,
                                         IApplicable<QuerySourceExpression>,
                                         IApplicable<NewExpression>,
                                         IApplicable<ColumnExpression>,
                                         IApplicable<RenameExpression>,
                                         IApplicable<BinaryExpression>,
                                         IApplicable<ConditionalExpression>,
                                         IApplicable<MethodCallExpression>,
                                         IApplicable<ParameterExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="type">Type</param>
        /// <param name="source">Source</param>
        /// <param name="parameter">Parameter</param>
        public NamedSourceExpression(
            Type type,
            ISqlExpression source,
            ISqlExpression parameter)
        {
            Type = type;
            Source = source;
            Parameter = parameter;
        }

        internal NamedSourceExpression(Type type, TranslationContext context)
            : this(type, null!, new ParameterExpression(context, type))
        {
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Source expression
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <summary>
        /// Parameter expression
        /// </summary>
        public ISqlExpression Parameter { get; }

        #region IApplicable

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
        public void Apply(TranslationContext context, OrderByExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, QuerySourceExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NewExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ColumnExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, RenameExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ConditionalExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, MethodCallExpression expression)
        {
            ForwardExpression(context, expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ParameterExpression expression)
        {
            ForwardExpression(context, expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (Source != null)
            {
                throw new InvalidOperationException("Source expression has already been set");
            }

            Source = expression;
        }

        private void ForwardExpression(TranslationContext context, ISqlExpression expression)
        {
            context.Apply(
                Source is FilterExpression filterExpression ? filterExpression.Source : Source,
                expression);
        }

        #endregion
    }
}