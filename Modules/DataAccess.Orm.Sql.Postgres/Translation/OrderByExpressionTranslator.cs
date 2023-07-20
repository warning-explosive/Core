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
    internal class OrderByExpressionTranslator : ISqlExpressionTranslator<OrderByExpression>,
                                                 IResolvable<ISqlExpressionTranslator<OrderByExpression>>,
                                                 ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        public OrderByExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is OrderByExpression orderByExpression
                ? Translate(orderByExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(OrderByExpression expression, int depth)
        {
            var sb = new StringBuilder();

            sb.AppendLine(_translator.Translate(expression.Source, depth));
            sb.AppendLine("ORDER BY");
            sb.Append(new string('\t', depth + 1));

            sb.Append(expression
                .Expressions
                .Select(sqlExpression => _translator.Translate(sqlExpression, depth))
                .ToString(", "));

            return sb.ToString();
        }
    }
}