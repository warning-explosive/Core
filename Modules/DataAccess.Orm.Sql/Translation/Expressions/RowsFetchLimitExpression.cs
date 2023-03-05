namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System;

    /// <summary>
    /// RowsFetchLimitExpression
    /// </summary>
    public class RowsFetchLimitExpression : ISqlExpression,
                                            IApplicable<ProjectionExpression>,
                                            IApplicable<FilterExpression>,
                                            IApplicable<JoinExpression>,
                                            IApplicable<NamedSourceExpression>
    {
        /// <summary> .cctor </summary>
        /// <param name="rowsFetchLimit">Rows fetch limit</param>
        /// <param name="source">Source expression</param>
        public RowsFetchLimitExpression(
            uint rowsFetchLimit,
            ISqlExpression source)
        {
            RowsFetchLimit = rowsFetchLimit;
            Source = source;
        }

        internal RowsFetchLimitExpression(uint rowsFetchLimit)
            : this(rowsFetchLimit, null!)
        {
            RowsFetchLimit = rowsFetchLimit;
        }

        /// <summary>
        /// Rows fetch limit
        /// </summary>
        public uint RowsFetchLimit { get; }

        /// <summary>
        /// Source
        /// </summary>
        public ISqlExpression Source { get; private set; }

        #region IApplicable

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
            if (Source != null)
            {
                throw new InvalidOperationException("Rows fetch limit expression source has already been set");
            }

            Source = expression;
        }

        #endregion
    }
}