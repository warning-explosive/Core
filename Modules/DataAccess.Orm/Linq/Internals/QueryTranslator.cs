namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
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
        private readonly IExpressionTranslator _translator;

        public QueryTranslator(IDependencyContainer dependencyContainer, IExpressionTranslator translator)
        {
            _dependencyContainer = dependencyContainer;
            _translator = translator;
        }

        public IQuery Translate(Expression expression)
        {
            var intermediateExpression = _translator.Translate(expression);

            /* TODO: extract parameters */

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