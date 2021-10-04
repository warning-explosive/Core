namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ParameterExpressionTranslator : IExpressionTranslator<ParameterExpression>
    {
        public string Translate(ParameterExpression expression, int depth)
        {
            return expression.Name;
        }
    }
}