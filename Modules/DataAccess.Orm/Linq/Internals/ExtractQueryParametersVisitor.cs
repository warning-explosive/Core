namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Internals
{
    using System.Collections.Generic;
    using Abstractions;
    using Expressions;

    internal class ExtractQueryParametersVisitor : IntermediateExpressionVisitorBase
    {
        internal ExtractQueryParametersVisitor()
        {
            QueryParameters = new List<QueryParameterExpression>();
        }

        internal List<QueryParameterExpression> QueryParameters { get; }

        protected override IIntermediateExpression VisitQueryParameter(QueryParameterExpression queryParameter)
        {
            QueryParameters.Add(queryParameter);
            return queryParameter;
        }
    }
}