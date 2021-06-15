namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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