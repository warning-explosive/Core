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
    internal class OrderByExpressionTranslator : ISqlExpressionTranslator<OrderByExpression>,
                                                 IResolvable<ISqlExpressionTranslator<OrderByExpression>>,
                                                 ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        public OrderByExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
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

            sb.AppendLine(_sqlExpressionTranslator.Translate(expression.Source, depth));
            sb.AppendLine("ORDER BY");
            sb.Append(new string('\t', depth + 1));

            expression
               .Bindings
               .Select(binding => _sqlExpressionTranslator.Translate(binding, depth))
               .Each((binding, i) =>
               {
                   if (i != 0)
                   {
                       sb.Append(", ");
                   }

                   sb.Append(binding);
               });

            return sb.ToString();
        }
    }
}