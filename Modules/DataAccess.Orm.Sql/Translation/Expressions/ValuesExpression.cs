namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions
{
    using System.Collections.Generic;

    /// <summary>
    /// ValuesExpression
    /// </summary>
    public class ValuesExpression : ISqlExpression,
                                    IApplicable<QueryParameterExpression>
    {
        private readonly List<QueryParameterExpression> _values;

        /// <summary> .cctor </summary>
        public ValuesExpression()
        {
            _values = new List<QueryParameterExpression>();
        }

        /// <summary>
        /// Values
        /// </summary>
        public IReadOnlyCollection<QueryParameterExpression> Values => _values;

        #region IApplicable

        /// <inheritdoc />
        public void Apply(TranslationContext context, QueryParameterExpression expression)
        {
            _values.Add(expression);
        }

        #endregion
    }
}