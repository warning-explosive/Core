namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using Expressions;

    internal class ExtractQueryParametersVisitor : SqlExpressionVisitorBase
    {
        private readonly Dictionary<string, object?> _queryParameters;

        public ExtractQueryParametersVisitor()
        {
            _queryParameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, object?> QueryParameters => _queryParameters;

        protected override ISqlExpression VisitQueryParameter(QueryParameterExpression queryParameter)
        {
            _queryParameters.Add(queryParameter.Name, queryParameter.Value);

            return queryParameter;
        }
    }
}