namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ExplainExpressionTranslator : ISqlExpressionTranslator<ExplainExpression>,
                                                 IResolvable<ISqlExpressionTranslator<ExplainExpression>>,
                                                 ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ExplainExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ExplainExpression explainExpression
                ? Translate(explainExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ExplainExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("EXPLAIN ");
            sb.Append("(");

            if (expression.Analyze)
            {
                sb.Append("ANALYZE, ");
            }

            sb.Append("FORMAT json");
            sb.AppendLine(")");

            sb.Append(_translator.Translate(expression.Source, depth));

            return sb.ToString();
        }
    }
}