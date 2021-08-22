namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Expressions;

    [Component(EnLifestyle.Scoped)]
    internal class GroupedQueryTranslator : IQueryTranslator<GroupByExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GroupedQueryTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task<IQuery> Translate(GroupByExpression intermediateExpression, CancellationToken token)
        {
            var keysQuery = await intermediateExpression.KeysExpression.Translate(_dependencyContainer, 0, token).ConfigureAwait(false);
            var keysQueryParameters = intermediateExpression.KeysExpression.ExtractQueryParameters(_dependencyContainer);

            return new GroupedQuery(keysQuery, keysQueryParameters, intermediateExpression.ValuesExpressionProducer);
        }
    }
}