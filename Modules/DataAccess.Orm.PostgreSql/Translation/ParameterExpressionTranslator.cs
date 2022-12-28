namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ParameterExpressionTranslator : ISqlExpressionTranslator<ParameterExpression>,
                                                   IResolvable<ISqlExpressionTranslator<ParameterExpression>>,
                                                   ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ParameterExpression parameterExpression
                ? Translate(parameterExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ParameterExpression expression, int depth)
        {
            return expression.Name;
        }
    }
}