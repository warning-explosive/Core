namespace SpaceEngineers.Core.DataAccess.Orm
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using ValueObjects;

    /// <summary>
    /// QueryTranslatorBase
    /// </summary>
    [Component(EnLifestyle.Scoped)]
    public abstract class QueryTranslatorBase : IQueryTranslator
    {
        private readonly IEnumerable<IQueryVisitor> _visitors;

        /// <summary> .cctor </summary>
        /// <param name="visitors">Query visitors</param>
        protected QueryTranslatorBase(IEnumerable<IQueryVisitor> visitors)
        {
            _visitors = visitors;
        }

        /// <inheritdoc />
        public TranslatedQuery Translate(Expression expression)
        {
            var visitedExpression = _visitors.Aggregate(expression, (current, visitor) => visitor.Apply(current));

            return TranslateVisited(visitedExpression);
        }

        /// <summary>
        /// Translate visited expression
        /// </summary>
        /// <param name="visitedExpression">Visited expression</param>
        /// <returns>Translated query</returns>
        protected abstract TranslatedQuery TranslateVisited(Expression visitedExpression);
    }
}