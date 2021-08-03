namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ConstantExpressionTranslator : IExpressionTranslator<ConstantExpression>
    {
        public string Translate(ConstantExpression expression, int depth)
        {
            return expression.Value?.ToString() ?? "NULL";
        }
    }
}