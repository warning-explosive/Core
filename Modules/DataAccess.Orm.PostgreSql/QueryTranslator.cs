namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using ValueObjects;

    [Component(EnLifestyle.Scoped)]
    internal class QueryTranslator : QueryTranslatorBase
    {
        public QueryTranslator(IEnumerable<IQueryVisitor> visitors)
            : base(visitors)
        {
        }

        protected override TranslatedQuery TranslateVisited(Expression visitedExpression)
        {
            _ = visitedExpression;

            throw new System.NotImplementedException();
        }
    }
}