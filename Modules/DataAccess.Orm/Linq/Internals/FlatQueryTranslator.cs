namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class FlatQueryTranslator<TExpression> : IQueryTranslator<TExpression>
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
            var queryParameters = intermediateExpression.ExtractQueryParameters(_dependencyContainer);

            return new FlatQuery(query, queryParameters);
        }
    }
}