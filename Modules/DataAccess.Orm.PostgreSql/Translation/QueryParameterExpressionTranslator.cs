namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QueryParameterExpressionTranslator : ISqlExpressionTranslator<QueryParameterExpression>,
                                                        IResolvable<ISqlExpressionTranslator<QueryParameterExpression>>,
                                                        ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is QueryParameterExpression queryParameterExpression
                ? Translate(queryParameterExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(QueryParameterExpression expression, int depth)
        {
            return $"@{expression.Name}";
        }
    }
}