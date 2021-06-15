namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql
{
    using System.Collections.Generic;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Internals;
    using Linq;
    using ValueObjects;

    /// <summary>
    /// QueryTranslator
    /// </summary>
    [Component(EnLifestyle.Scoped)]
    public class QueryTranslator : QueryTranslatorBase
    {
        private readonly IDependencyContainer _dependencyContainer;

        /// <summary>
        /// .cctor
        /// </summary>
        /// <param name="dependencyContainer">IDependencyContainer</param>
        /// <param name="queryVisitors">IQueryVisitor stream</param>
        /// <param name="translator">IIntermediateTranslator</param>
        public QueryTranslator(
            IDependencyContainer dependencyContainer,
            IEnumerable<IQueryVisitor> queryVisitors,
            IExpressionTranslator translator)
            : base(queryVisitors, translator)
        {
            _dependencyContainer = dependencyContainer;
        }

        /// <inheritdoc />
        protected override TranslatedQuery TranslateVisited(IIntermediateExpression expression)
        {
            var query = expression.Translate(_dependencyContainer, 0);
            return new TranslatedQuery(query);
        }
    }
}