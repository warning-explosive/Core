namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ParenthesesExpressionTranslator : ISqlExpressionTranslator<ParenthesesExpression>,
                                                     IResolvable<ISqlExpressionTranslator<ParenthesesExpression>>,
                                                     ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ParenthesesExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ParenthesesExpression parenthesesExpression
                ? Translate(parenthesesExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ParenthesesExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append('(');
            sb.Append(_translator.Translate(expression.Source, depth));
            sb.Append(')');

            return sb.ToString();
        }
    }
}