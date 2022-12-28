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
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public FilterExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
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

            sb.AppendLine(_sqlExpressionTranslator.Translate(expression.Source, depth));
            sb.Append(new string('\t', depth));
            sb.AppendLine("WHERE");
            sb.Append(new string('\t', depth + 1));
            sb.Append(_sqlExpressionTranslator.Translate(expression.Predicate, depth));

            return sb.ToString();
        }
    }
}