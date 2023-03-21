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
    internal class OrderByExpressionExpressionTranslator : ISqlExpressionTranslator<OrderByExpressionExpression>,
                                                           IResolvable<ISqlExpressionTranslator<OrderByExpressionExpression>>,
                                                           ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public OrderByExpressionExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is OrderByExpressionExpression orderByExpressionExpression
                ? Translate(orderByExpressionExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(OrderByExpressionExpression expressionExpression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_translator.Translate(expressionExpression.Expression, depth));
            sb.Append(' ');
            sb.Append(expressionExpression.OrderingDirection.ToString().ToUpperInvariant());

            return sb.ToString();
        }
    }
}