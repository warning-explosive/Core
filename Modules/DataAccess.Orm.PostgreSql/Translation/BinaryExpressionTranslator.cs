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

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : ISqlExpressionTranslator<BinaryExpression>,
                                                IResolvable<ISqlExpressionTranslator<BinaryExpression>>,
                                                ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly ISqlExpressionTranslatorComposite _translator;

        private static readonly IReadOnlyDictionary<BinaryOperator, string> FunctionalOperators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Coalesce] = "COALESCE"
            };

        private static readonly IReadOnlyDictionary<BinaryOperator, string> Operators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Assign] = "=",
                [BinaryOperator.Equal] = "=",
                [BinaryOperator.NotEqual] = "!=",
                [BinaryOperator.Is] = "IS",
                [BinaryOperator.IsNot] = "IS NOT",
                [BinaryOperator.GreaterThanOrEqual] = ">=",
                [BinaryOperator.GreaterThan] = ">",
                [BinaryOperator.LessThan] = "<",
                [BinaryOperator.LessThanOrEqual] = "<=",
                [BinaryOperator.AndAlso] = "AND",
                [BinaryOperator.OrElse] = "OR",
                [BinaryOperator.ExclusiveOr] = "XOR",
                [BinaryOperator.Like] = "LIKE",
                [BinaryOperator.Add] = "+",
                [BinaryOperator.Subtract] = "-",
                [BinaryOperator.Divide] = "/",
                [BinaryOperator.Multiply] = "*",
                [BinaryOperator.Modulo] = "%",
                [BinaryOperator.HasJsonAttribute] = "?",
                [BinaryOperator.ConcatJsonObjects] = "||",
                [BinaryOperator.ArrayIntersection] = "&&"
            };

        public BinaryExpressionTranslator(ISqlExpressionTranslatorComposite translator)
        {
            _translator = translator;
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

            if (expression.Operator == BinaryOperator.Contains)
            {
                sb.Append(_translator.Translate(expression.Left, depth));
                sb.Append(" = ANY");
                sb.Append('(');
                sb.Append(_translator.Translate(expression.Right, depth + 1));
                sb.Append(')');
            }
            else if (FunctionalOperators.ContainsKey(expression.Operator))
            {
                sb.Append(FunctionalOperators[expression.Operator]);
                sb.Append('(');
                sb.Append(_translator.Translate(expression.Left, depth));
                sb.Append(", ");
                sb.Append(_translator.Translate(expression.Right, depth));
                sb.Append(')');
            }
            else
            {
                sb.Append(_translator.Translate(expression.Left, depth));
                sb.Append(" ");
                sb.Append(Operators[expression.Operator]);
                sb.Append(" ");
                sb.Append(_translator.Translate(expression.Right, depth + 1));
            }

            return sb.ToString();
        }
    }
}