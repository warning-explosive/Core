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
    internal class NamedBindingExpressionTranslator : ISqlExpressionTranslator<NamedBindingExpression>,
                                                      IResolvable<ISqlExpressionTranslator<NamedBindingExpression>>,
                                                      ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public NamedBindingExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is NamedBindingExpression namedBindingExpression
                ? Translate(namedBindingExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(NamedBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            var parentheses = expression.Source is not SimpleBindingExpression;

            if (parentheses)
            {
                sb.Append('(');
            }

            sb.Append(_sqlExpressionTranslator.Translate(expression.Source, depth));

            if (parentheses)
            {
                sb.Append(')');
            }

            sb.Append(" AS ");
            sb.Append('"');
            sb.Append(expression.Name);
            sb.Append('"');

            return sb.ToString();
        }
    }
}