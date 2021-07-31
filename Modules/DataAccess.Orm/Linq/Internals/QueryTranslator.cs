namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Scoped)]
    internal class QueryTranslator : IQueryTranslator
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IEnumerable<IQueryVisitor> _queryVisitors;
        private readonly IExpressionTranslator _translator;

        public QueryTranslator(
            IDependencyContainer dependencyContainer,
            IEnumerable<IQueryVisitor> queryVisitors,
            IExpressionTranslator translator)
        {
            _dependencyContainer = dependencyContainer;
            _queryVisitors = queryVisitors;
            _translator = translator;
        }

        public IQuery Translate(Expression expression)
        {
            var visitedExpression = _queryVisitors.Aggregate(expression, (current, visitor) => visitor.Apply(current));

            var intermediateExpression = _translator.Translate(visitedExpression);

            if (intermediateExpression is GroupByExpression groupByExpression)
            {
                var keysQuery = groupByExpression.Keys.Translate(_dependencyContainer, 0);
                var valuesQuery = groupByExpression.Values.Translate(_dependencyContainer, 0);
                return new GroupedQuery(keysQuery, valuesQuery);
            }

            var query = intermediateExpression.Translate(_dependencyContainer, 0);
            return new FlatQuery(query);
        }
    }
}