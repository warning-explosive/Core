namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions;

    /// <summary>
    /// QueryTranslatorBase
    /// </summary>
    public abstract class QueryTranslatorBase : IQueryTranslator
    {
        private readonly IEnumerable<IQueryVisitor> _queryVisitors;
        private readonly IExpressionTranslator _translator;

        /// <summary> .cctor </summary>
        /// <param name="queryVisitors">Query visitors</param>
        /// <param name="translator">IIntermediateTranslator</param>
        protected QueryTranslatorBase(
            IEnumerable<IQueryVisitor> queryVisitors,
            IExpressionTranslator translator)
        {
            _queryVisitors = queryVisitors;
            _translator = translator;
        }

        /// <inheritdoc />
        public TranslatedQuery Translate(Expression expression)
        {
            var visitedExpression = _queryVisitors.Aggregate(expression, (current, visitor) => visitor.Apply(current));

            var intermediateExpression = _translator.Translate(visitedExpression);

            return TranslateVisited(intermediateExpression);
        }

        /// <summary>
        /// Translate visited expression
        /// </summary>
        /// <param name="expression">Intermediate expression</param>
        /// <returns>Translated query</returns>
        protected abstract TranslatedQuery TranslateVisited(IIntermediateExpression expression);
    }
}