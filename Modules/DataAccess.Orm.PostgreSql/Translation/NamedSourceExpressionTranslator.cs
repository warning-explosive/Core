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
    internal class NamedSourceExpressionTranslator : ISqlExpressionTranslator<NamedSourceExpression>,
                                                     IResolvable<ISqlExpressionTranslator<NamedSourceExpression>>,
                                                     ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public NamedSourceExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is NamedSourceExpression namedSourceExpression
                ? Translate(namedSourceExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(NamedSourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parenthesis = expression.Source is not QuerySourceExpression;

            if (parenthesis)
            {
                sb.Append('(');
            }

            sb.Append(_translator.Translate(expression.Source, depth));

            if (parenthesis)
            {
                sb.Append(')');
            }

            sb.Append(' ');
            sb.Append(_translator.Translate(expression.Parameter, depth));

            return sb.ToString();
        }
    }
}