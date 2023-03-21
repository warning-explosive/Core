namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class ValuesExpressionTranslator : ISqlExpressionTranslator<ValuesExpression>,
                                                IResolvable<ISqlExpressionTranslator<ValuesExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public ValuesExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is ValuesExpression valuesExpression
                ? Translate(valuesExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(ValuesExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append("(");

            sb.Append(expression
                .Values
                .Select(value => _translator.Translate(value, depth))
                .ToString(", "));

            sb.Append(")");

            return sb.ToString();
        }
    }
}