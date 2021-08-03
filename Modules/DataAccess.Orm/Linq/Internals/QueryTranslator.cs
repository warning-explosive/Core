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

            if (intermediateExpression is GroupByExpression groupByExpression)
            {
                var keyQuery = groupByExpression.Keys.Translate(_dependencyContainer, 0);
                var keyQueryParameters = groupByExpression.Keys.ExtractParameters(_dependencyContainer);

                var valueQuery = groupByExpression.Values.Translate(_dependencyContainer, 0);
                var valueQueryParameters = groupByExpression.Values.ExtractParameters(_dependencyContainer);

                return new GroupedQuery(keyQuery, keyQueryParameters, valueQuery, valueQueryParameters);
            }
            else
            {
                var query = intermediateExpression.Translate(_dependencyContainer, 0);
                var queryParameters = intermediateExpression.ExtractParameters(_dependencyContainer);

                return new FlatQuery(query, queryParameters);
            }
        }
    }
}