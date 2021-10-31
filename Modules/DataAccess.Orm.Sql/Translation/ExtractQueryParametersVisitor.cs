namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractQueryParametersVisitor : IntermediateExpressionVisitorBase
    {
        private readonly List<QueryParameterExpression> _queryParameters;

        private ExtractQueryParametersVisitor()
        {
            _queryParameters = new List<QueryParameterExpression>();
        }

        public static IReadOnlyCollection<QueryParameterExpression> ExtractQueryParameters(IIntermediateExpression expression)
        {
            var extractor = new ExtractQueryParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor._queryParameters;
        }

        protected override IIntermediateExpression VisitQueryParameter(QueryParameterExpression queryParameter)
        {
            _queryParameters.Add(queryParameter);
            return queryParameter;
        }
    }
}