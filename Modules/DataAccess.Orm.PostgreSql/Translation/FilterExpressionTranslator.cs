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
    internal class FilterExpressionTranslator : ISqlExpressionTranslator<FilterExpression>,
                                                IResolvable<ISqlExpressionTranslator<FilterExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public FilterExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is FilterExpression filterExpression
                ? Translate(filterExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(FilterExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_translator.Translate(expression.Source, depth));
            sb.Append(new string('\t', depth));
            sb.AppendLine("WHERE");
            sb.Append(new string('\t', depth + 1));
            sb.Append(_translator.Translate(expression.Predicate, depth));

            return sb.ToString();
        }
    }
}