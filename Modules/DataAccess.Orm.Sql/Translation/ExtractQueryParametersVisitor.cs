namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractQueryParametersVisitor : IntermediateExpressionVisitorBase
    {
        public ExtractQueryParametersVisitor()
        {
            QueryParameters = new List<QueryParameterExpression>();
        }

        public List<QueryParameterExpression> QueryParameters { get; }

        protected override IIntermediateExpression VisitQueryParameter(QueryParameterExpression queryParameter)
        {
            QueryParameters.Add(queryParameter);
            return queryParameter;
        }
    }
}