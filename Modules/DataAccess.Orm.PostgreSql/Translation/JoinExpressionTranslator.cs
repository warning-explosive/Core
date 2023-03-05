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
    internal class JoinExpressionTranslator : ISqlExpressionTranslator<JoinExpression>,
                                              IResolvable<ISqlExpressionTranslator<JoinExpression>>,
                                              ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public JoinExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is JoinExpression joinExpression
                ? Translate(joinExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(JoinExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_translator.Translate(expression.LeftSource, depth));
            sb.Append(new string('\t', Math.Max(depth - 1, 0)));
            sb.AppendLine("JOIN");
            sb.Append(new string('\t', depth));
            sb.AppendLine(_translator.Translate(expression.RightSource, depth));
            sb.Append(new string('\t', Math.Max(depth - 1, 0)));
            sb.AppendLine("ON");
            sb.Append(new string('\t', depth));
            sb.Append(_translator.Translate(expression.On, depth));

            return sb.ToString();
        }
    }
}