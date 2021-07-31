namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Internals;
    using BinaryExpression = Linq.Expressions.BinaryExpression;
    using ConstantExpression = Linq.Expressions.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>
    {
        private static readonly IReadOnlyDictionary<string, string> FunctionalOperators
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(ExpressionType.Coalesce)] = "COALESCE"
            };

        private static readonly IReadOnlyDictionary<string, string> Operators
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(ExpressionType.Equal)] = "=",
                [nameof(ExpressionType.NotEqual)] = "!=",
                [nameof(ExpressionType.GreaterThanOrEqual)] = ">=",
                [nameof(ExpressionType.GreaterThan)] = ">",
                [nameof(ExpressionType.LessThan)] = "<",
                [nameof(ExpressionType.LessThanOrEqual)] = "<=",
                [nameof(ExpressionType.AndAlso)] = "AND",
                [nameof(ExpressionType.OrElse)] = "OR",
                [nameof(ExpressionType.ExclusiveOr)] = "XOR"
            };

        private static readonly IReadOnlyDictionary<string, string> AltOperators
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(ExpressionType.Equal)] = "IS",
                [nameof(ExpressionType.NotEqual)] = "IS NOT"
            };

        private readonly IDependencyContainer _dependencyContainer;

        public BinaryExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(BinaryExpression expression, int depth)
        {
            if (FunctionalOperators.ContainsKey(expression.Operator))
            {
                var sb = new StringBuilder();

                sb.Append(FunctionalOperators[expression.Operator]);
                sb.Append('(');
                sb.Append(expression.Left.Translate(_dependencyContainer, depth));
                sb.Append(", ");
                sb.Append(expression.Right.Translate(_dependencyContainer, depth));
                sb.Append(')');

                return sb.ToString();
            }

            var map = (IsNullConstant(expression.Left) || IsNullConstant(expression.Right))
                      && AltOperators.ContainsKey(expression.Operator)
                ? AltOperators
                : Operators;

            return string.Join(" ",
                expression.Left.Translate(_dependencyContainer, depth),
                map[expression.Operator],
                expression.Right.Translate(_dependencyContainer, depth));
        }

        private static bool IsNullConstant(IIntermediateExpression expression)
        {
            return expression is ConstantExpression { Value: null };
        }
    }
}