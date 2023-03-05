namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// JoinExpression
    /// </summary>
    public class JoinExpression : ISqlExpression,
                                  IApplicable<JoinExpression>,
                                  IApplicable<NamedSourceExpression>,
                                  IApplicable<BinaryExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="leftSource">Left source expression</param>
        /// <param name="rightSource">Right source expression</param>
        /// <param name="on">On expression</param>
        public JoinExpression(
            ISqlExpression leftSource,
            ISqlExpression rightSource,
            ISqlExpression on)
        {
            LeftSource = leftSource;
            RightSource = rightSource;
            On = on;
        }

        internal JoinExpression()
            : this(null!, null!, null!)
        {
        }

        /// <summary>
        /// Left source expression
        /// </summary>
        public ISqlExpression LeftSource { get; private set; }

        /// <summary>
        /// Right source expression
        /// </summary>
        public ISqlExpression RightSource { get; private set; }

        /// <summary>
        /// On expression
        /// </summary>
        public ISqlExpression On { get; private set; }

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, BinaryExpression expression)
        {
            if (On != null)
            {
                throw new InvalidOperationException("Join on expression has already been set");
            }

            On = expression;
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, JoinExpression expression)
        {
            ApplySource(expression);
        }

        /// <inheritdoc />
        public void Apply(TranslationContext context, NamedSourceExpression expression)
        {
            ApplySource(expression);
        }

        private void ApplySource(ISqlExpression expression)
        {
            if (LeftSource == null)
            {
                LeftSource = expression;
                return;
            }

            if (RightSource == null)
            {
                RightSource = expression;
                return;
            }

            throw new InvalidOperationException("Join expression sources have already been set");
        }

        #endregion
    }
}