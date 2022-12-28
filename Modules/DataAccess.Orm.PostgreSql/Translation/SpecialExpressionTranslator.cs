namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]

    internal class SpecialExpressionTranslator : ISqlExpressionTranslator<SpecialExpression>,
                                                 IResolvable<ISqlExpressionTranslator<SpecialExpression>>,
                                                 ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is SpecialExpression specialExpression
                ? Translate(specialExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(SpecialExpression expression, int depth)
        {
            return expression.Text;
        }
    }
}