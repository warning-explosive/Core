namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot;
    using Expressions;
    using Extensions;
    using Orm.Linq;

    [Component(EnLifestyle.Scoped)]
    internal class GroupedQueryTranslator : IIntermediateQueryTranslator<GroupByExpression>,
                                            IResolvable<IIntermediateQueryTranslator<GroupByExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GroupedQueryTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public IQuery Translate(GroupByExpression intermediateExpression)
        {
            var keysQuery = intermediateExpression.KeysExpression.Translate(_dependencyContainer, 0);
            var keysQueryParameters = intermediateExpression.KeysExpression.ExtractQueryParameters();

            return new GroupedQuery(keysQuery, keysQueryParameters, intermediateExpression.ValuesExpressionProducer);
        }
    }
}