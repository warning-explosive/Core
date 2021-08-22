namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QueryParameterExpressionTranslator : IExpressionTranslator<QueryParameterExpression>
    {
        public Task<string> Translate(QueryParameterExpression expression, int depth, CancellationToken token)
        {
            return Task.FromResult($"@{expression.Name}");
        }
    }
}