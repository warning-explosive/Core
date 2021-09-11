namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ParameterExpressionTranslator : IExpressionTranslator<ParameterExpression>
    {
        public Task<string> Translate(ParameterExpression expression, int depth, CancellationToken token)
        {
            return Task.FromResult(expression.Name);
        }
    }
}