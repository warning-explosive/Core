namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryTranslator<TExpression> : IQueryTranslator<TExpression>
        where TExpression : IIntermediateExpression
    {
        private readonly IDependencyContainer _dependencyContainer;

        public FlatQueryTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public IQuery Translate(TExpression intermediateExpression)
        {
            var query = intermediateExpression.Translate(_dependencyContainer, 0);
            var queryParameters = intermediateExpression.ExtractQueryParameters(_dependencyContainer);

            return new FlatQuery(query, queryParameters);
        }
    }
}