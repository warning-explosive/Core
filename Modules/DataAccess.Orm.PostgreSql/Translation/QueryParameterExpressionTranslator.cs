namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QueryParameterExpressionTranslator : IExpressionTranslator<QueryParameterExpression>
    {
        public Task<string> Translate(QueryParameterExpression expression, int depth, CancellationToken token)
        {
            return Task.FromResult($"@{expression.Name}");
        }
    }
}