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
    internal class OrderByBindingExpressionTranslator : ISqlExpressionTranslator<OrderByBindingExpression>,
                                                        IResolvable<ISqlExpressionTranslator<OrderByBindingExpression>>,
                                                        ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public OrderByBindingExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is OrderByBindingExpression orderByBindingExpression
                ? Translate(orderByBindingExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(OrderByBindingExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.Append(_sqlExpressionTranslator.Translate(expression.Binding, depth));
            sb.Append(' ');
            sb.Append(expression.OrderingDirection.ToString().ToUpperInvariant());

            return sb.ToString();
        }
    }
}