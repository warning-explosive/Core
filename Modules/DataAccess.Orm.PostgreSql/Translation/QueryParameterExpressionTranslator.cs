namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QueryParameterExpressionTranslator : IExpressionTranslator<QueryParameterExpression>
    {
        public string Translate(QueryParameterExpression expression, int depth)
        {
            return $"@{expression.Name}";
        }
    }
}