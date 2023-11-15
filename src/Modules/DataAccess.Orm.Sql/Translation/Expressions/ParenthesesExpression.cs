namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// ParenthesesExpression
    /// </summary>
    public class ParenthesesExpression : ISqlExpression,
                                         IApplicable<BinaryExpression>,
                                         IApplicable<JsonAttributeExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="source">Source</param>
        public ParenthesesExpression(ISqlExpression source)
        {
            Source = source;
        }

        internal ParenthesesExpression()
            : this(null!)
        {
        }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JsonAttributeExpression expression)
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

        #endregion
    }
}