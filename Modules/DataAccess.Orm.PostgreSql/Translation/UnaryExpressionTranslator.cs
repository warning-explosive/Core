namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class UnaryExpressionTranslator : ISqlExpressionTranslator<UnaryExpression>,
                                               IResolvable<ISqlExpressionTranslator<UnaryExpression>>,
                                               ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        private static readonly IReadOnlyDictionary<UnaryOperator, string> Operators
            = new Dictionary<UnaryOperator, string>
            {
                [UnaryOperator.Not] = "NOT"
            };

        public UnaryExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is UnaryExpression unaryExpression
                ? Translate(unaryExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(UnaryExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(Operators[expression.Operator]);
            sb.Append(" ");
            sb.Append(_sqlExpressionTranslator.Translate(expression.Source, depth));

            return sb.ToString();
        }
    }
}