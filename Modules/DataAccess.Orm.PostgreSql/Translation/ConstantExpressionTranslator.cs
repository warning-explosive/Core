namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ConstantExpressionTranslator : IExpressionTranslator<ConstantExpression>,
                                                  IResolvable<IExpressionTranslator<ConstantExpression>>
    {
        public string Translate(ConstantExpression expression, int depth)
        {
            return expression.Value?.ToString() ?? "NULL";
        }
    }
}