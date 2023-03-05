namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ColumnExpressionTranslator : ISqlExpressionTranslator<ColumnExpression>,
                                                IResolvable<ISqlExpressionTranslator<ColumnExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ColumnExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ColumnExpression columnExpression
                ? Translate(columnExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ColumnExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var source = expression
               .Flatten()
               .LastOrDefault()
               .Source;

            if (source != null)
            {
                sb.Append(_translator.Translate(source, depth));
                sb.Append('.');
            }

            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}