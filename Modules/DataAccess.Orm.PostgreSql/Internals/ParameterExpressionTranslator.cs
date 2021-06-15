namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
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