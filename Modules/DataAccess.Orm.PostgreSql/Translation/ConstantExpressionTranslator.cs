namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ConstantExpressionTranslator : IExpressionTranslator<ConstantExpression>
    {
        public Task<string> Translate(ConstantExpression expression, int depth, CancellationToken token)
        {
            return Task.FromResult(expression.Value?.ToString() ?? "NULL");
        }
    }
}