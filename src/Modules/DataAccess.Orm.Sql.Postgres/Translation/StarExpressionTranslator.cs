namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]

    internal class StarExpressionTranslator : ISqlExpressionTranslator<StarExpression>,
                                              IResolvable<ISqlExpressionTranslator<StarExpression>>,
                                              ICollectionResolvable<ISqlExpressionTranslator>
    {
        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is StarExpression starExpression
                ? Translate(starExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(StarExpression expression, int depth)
        {
            return "*";
        }
    }
}