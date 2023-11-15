namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Postgres.Translation
{
    using System;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class MethodCallExpressionTranslator : ISqlExpressionTranslator<MethodCallExpression>,
                                                    IResolvable<ISqlExpressionTranslator<MethodCallExpression>>,
                                                    ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public MethodCallExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
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
                sb.Append(_translator.Translate(expression.Source, depth));
                sb.Append('.');
            }

            sb.Append(expression.Name);

            sb.Append('(');

            var arguments = expression
                .Arguments
                .Select(argument => _translator.Translate(argument, depth));

            sb.Append(arguments.ToString(", "));

            sb.Append(')');

            return sb.ToString();
        }
    }
}