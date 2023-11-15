namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ExplainExpression
    /// </summary>
    public class ExplainExpression : ISqlExpression,
                                     IApplicable<ProjectionExpression>,
                                     IApplicable<FilterExpression>,
                                     IApplicable<OrderByExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="analyze">Analyze</param>
        /// <param name="source">Source</param>
        public ExplainExpression(bool analyze, ISqlExpression source)
        {
            Analyze = analyze;
            Source = source;
        }

        internal ExplainExpression(bool analyze)
            : this(analyze, null!)
        {
        }

        /// <summary>
        /// Analyze
        /// </summary>
        public bool Analyze { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        /// <inheritdoc />
        public void Apply(TranslationContext context, ProjectionExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, FilterExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, OrderByExpression expression)
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
    }
}