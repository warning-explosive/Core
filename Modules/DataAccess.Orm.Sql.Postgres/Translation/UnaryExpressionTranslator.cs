namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class UnaryExpressionTranslator : ISqlExpressionTranslator<UnaryExpression>,
                                               IResolvable<ISqlExpressionTranslator<UnaryExpression>>,
                                               ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        private static readonly IReadOnlyDictionary<UnaryOperator, string> Operators
            = new Dictionary<UnaryOperator, string>
            {
                [UnaryOperator.Not] = "NOT"
            };

        public UnaryExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
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
            sb.Append(' ');
            sb.Append(_translator.Translate(expression.Source, depth));

            return sb.ToString();
        }
    }
}