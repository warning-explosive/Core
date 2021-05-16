namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using BinaryExpression = ValueObjects.BinaryExpression;
    using ConstantExpression = ValueObjects.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>
    {
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
            var map = IsNullConstant(expression.Left)
                      || IsNullConstant(expression.Right)
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