namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;

    /// <summary>
    /// BatchExpression
    /// </summary>
    public class BatchExpression : ISqlExpression,
                                   IApplicable<InsertExpression>
    {
        private readonly List<ISqlExpression> _expressions;

        /// <summary> .cctor </summary>
        public BatchExpression()
        {
            _expressions = new List<ISqlExpression>();
        }

        /// <summary>
        /// Expressions
        /// </summary>
        public IReadOnlyCollection<ISqlExpression> Expressions => _expressions;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, InsertExpression expression)
        {
            ApplyExpression(expression);
        }

        private void ApplyExpression(InsertExpression expression)
        {
            _expressions.Add(expression);
        }

        #endregion
    }
}