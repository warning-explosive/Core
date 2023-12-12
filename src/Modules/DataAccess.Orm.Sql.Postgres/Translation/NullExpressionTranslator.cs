namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]

    internal class NullExpressionTranslator : ISqlExpressionTranslator<NullExpression>,
                                              IResolvable<ISqlExpressionTranslator<NullExpression>>,
                                              ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is NullExpression nullExpression
                ? Translate(nullExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(NullExpression expression, int depth)
        {
            return "NULL";
        }
    }
}