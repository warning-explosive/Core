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
    internal class ConditionalExpressionTranslator : ISqlExpressionTranslator<ConditionalExpression>,
                                                     IResolvable<ISqlExpressionTranslator<ConditionalExpression>>,
                                                     ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ConditionalExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ConditionalExpression conditionalExpression
                ? Translate(conditionalExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ConditionalExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("CASE WHEN ");
            sb.Append(_translator.Translate(expression.When, depth));
            sb.Append(" THEN ");
            sb.Append(_translator.Translate(expression.Then, depth));
            sb.Append(" ELSE ");
            sb.Append(_translator.Translate(expression.Else, depth));
            sb.Append(" END");

            return sb.ToString();
        }
    }
}