namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]

    internal class SpecialExpressionTranslator : IExpressionTranslator<SpecialExpression>
    {
        public string Translate(SpecialExpression expression, int depth)
        {
            return expression.Text;
        }
    }
}