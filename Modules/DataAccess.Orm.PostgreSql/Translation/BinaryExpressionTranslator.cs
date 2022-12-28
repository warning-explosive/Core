namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using BinaryExpression = Sql.Translation.Expressions.BinaryExpression;
    using ConstantExpression = Sql.Translation.Expressions.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : ISqlExpressionTranslator<BinaryExpression>,
                                                IResolvable<ISqlExpressionTranslator<BinaryExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _sqlExpressionTranslator;

        private static readonly IReadOnlyDictionary<BinaryOperator, string> FunctionalOperators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Coalesce] = "COALESCE"
            };

        private static readonly IReadOnlyDictionary<BinaryOperator, string> Operators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Equal] = "=",
                [BinaryOperator.NotEqual] = "!=",
                [BinaryOperator.GreaterThanOrEqual] = ">=",
                [BinaryOperator.GreaterThan] = ">",
                [BinaryOperator.LessThan] = "<",
                [BinaryOperator.LessThanOrEqual] = "<=",
                [BinaryOperator.AndAlso] = "AND",
                [BinaryOperator.OrElse] = "OR",
                [BinaryOperator.ExclusiveOr] = "XOR",
                [BinaryOperator.Contains] = "IN",
                [BinaryOperator.Like] = "LIKE",
                [BinaryOperator.Add] = "+",
                [BinaryOperator.Subtract] = "-",
                [BinaryOperator.Divide] = "/",
                [BinaryOperator.Multiply] = "*",
                [BinaryOperator.Modulo] = "%"
            };

        private static readonly IReadOnlyDictionary<BinaryOperator, string> AltOperators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Equal] = "IS",
                [BinaryOperator.NotEqual] = "IS NOT"
            };

        public BinaryExpressionTranslator(ISqlExpressionTranslatorComposite sqlExpressionTranslatorComposite)
        {
            _sqlExpressionTranslator = sqlExpressionTranslatorComposite;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is BinaryExpression binaryExpression
                ? Translate(binaryExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(BinaryExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (FunctionalOperators.ContainsKey(expression.Operator))
            {
                sb.Append(FunctionalOperators[expression.Operator]);
                sb.Append('(');
                sb.Append(_sqlExpressionTranslator.Translate(expression.Left, depth));
                sb.Append(", ");
                sb.Append(_sqlExpressionTranslator.Translate(expression.Right, depth));
                sb.Append(')');
            }
            else
            {
                var map = (IsNullConstant(expression.Left) || IsNullConstant(expression.Right))
                          && AltOperators.ContainsKey(expression.Operator)
                    ? AltOperators
                    : Operators;

                sb.Append(_sqlExpressionTranslator.Translate(expression.Left, depth));
                sb.Append(" ");
                sb.Append(map[expression.Operator]);

                if (expression.Operator == BinaryOperator.Contains)
                {
                    sb.Append(" ");
                    sb.Append('(');
                }
                else
                {
                    sb.Append(" ");
                }

                sb.Append(_sqlExpressionTranslator.Translate(expression.Right, depth + 1));

                if (expression.Operator == BinaryOperator.Contains)
                {
                    sb.Append(')');
                }
            }

            return sb.ToString();
        }

        private static bool IsNullConstant(ISqlExpression expression)
        {
            return expression is ConstantExpression { Value: null };
        }
    }
}