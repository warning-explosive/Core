namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Expressions;
    using Extensions;
    using Orm.Linq;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryTranslator<TExpression> : IIntermediateQueryTranslator<TExpression>
        where TExpression : IIntermediateExpression
    {
        private readonly IDependencyContainer _dependencyContainer;

        public FlatQueryTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<IQuery> Translate(TExpression intermediateExpression, CancellationToken token)
        {
            var query = await intermediateExpression.Translate(_dependencyContainer, 0, token).ConfigureAwait(false);
            var queryParameters = intermediateExpression.ExtractQueryParameters();

            return new FlatQuery(query, queryParameters);
        }
    }
}