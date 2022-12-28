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
    internal class MethodCallExpressionTranslator : ISqlExpressionTranslator<MethodCallExpression>,
                                                    IResolvable<ISqlExpressionTranslator<MethodCallExpression>>,
                                                    ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public MethodCallExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is MethodCallExpression methodCallExpression
                ? Translate(methodCallExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(MethodCallExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (expression.Source != null)
            {
                sb.Append(_sqlExpressionTranslator.Translate(expression.Source, depth));
                sb.Append('.');
            }

            sb.Append(expression.Name);

            sb.Append('(');

            var arguments = expression
                .Arguments
                .Select(argument => _sqlExpressionTranslator.Translate(argument, depth));

            sb.Append(arguments.ToString(", "));

            sb.Append(')');

            return sb.ToString();
        }
    }
}