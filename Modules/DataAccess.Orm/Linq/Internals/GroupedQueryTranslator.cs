namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Expressions;

    [Component(EnLifestyle.Scoped)]
    internal class GroupedQueryTranslator : IQueryTranslator<GroupByExpression>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GroupedQueryTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public IQuery Translate(GroupByExpression intermediateExpression)
        {
            var keysQuery = intermediateExpression.KeysExpression.Translate(_dependencyContainer, 0);
            var keysQueryParameters = intermediateExpression.KeysExpression.ExtractQueryParameters(_dependencyContainer);

            return new GroupedQuery(keysQuery, keysQueryParameters, intermediateExpression.ValuesExpressionProducer);
        }
    }
}