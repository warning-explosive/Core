namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Globalization;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class RowsFetchLimitExpressionTranslator : ISqlExpressionTranslator<RowsFetchLimitExpression>,
                                                        IResolvable<ISqlExpressionTranslator<RowsFetchLimitExpression>>,
                                                        ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public RowsFetchLimitExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is RowsFetchLimitExpression rowsFetchLimitExpression
                ? Translate(rowsFetchLimitExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(RowsFetchLimitExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(new string('\t', depth));
            sb.AppendLine(_sqlExpressionTranslator.Translate(expression.Source, depth));

            sb.Append(new string('\t', depth));
            sb.Append(CultureInfo.InvariantCulture, $"fetch first {expression.RowsFetchLimit} rows only");

            return sb.ToString();
        }
    }
}