namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ParameterExpressionTranslator : IExpressionTranslator<ParameterExpression>
    {
        public string Translate(ParameterExpression expression, int depth)
        {
            return expression.Name;
        }
    }
}