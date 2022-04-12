namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Expressions;
    using Extensions;
    using Orm.Linq;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryTranslator<TExpression> : IIntermediateQueryTranslator<TExpression>,
                                                      IResolvable<IIntermediateQueryTranslator<TExpression>>
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
            var queryParameters = intermediateExpression.ExtractQueryParameters();

            return new FlatQuery(query, queryParameters);
        }
    }
}