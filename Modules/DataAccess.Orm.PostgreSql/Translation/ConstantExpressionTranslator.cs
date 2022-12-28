namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ConstantExpressionTranslator : ISqlExpressionTranslator<ConstantExpression>,
                                                  IResolvable<ISqlExpressionTranslator<ConstantExpression>>,
                                                  ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ConstantExpression constantExpression
                ? Translate(constantExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ConstantExpression expression, int depth)
        {
            return expression.Value?.ToString() ?? "NULL";
        }
    }
}