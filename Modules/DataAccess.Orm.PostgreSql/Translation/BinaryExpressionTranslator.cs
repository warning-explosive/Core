namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Linq.Abstractions;
    using Linq.Internals;
    using BinaryExpression = Linq.Expressions.BinaryExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>
    {
        private static readonly IReadOnlyDictionary<ExpressionType, string> FunctionalOperators
            = new Dictionary<ExpressionType, string>()
            {
                [ExpressionType.Coalesce] = "COALESCE"
            };

        private static readonly IReadOnlyDictionary<ExpressionType, string> Operators
            = new Dictionary<ExpressionType, string>()
            {
                [ExpressionType.Equal] = "=",
                [ExpressionType.NotEqual] = "!=",
                [ExpressionType.GreaterThanOrEqual] = ">=",
                [ExpressionType.GreaterThan] = ">",
                [ExpressionType.LessThan] = "<",
                [ExpressionType.LessThanOrEqual] = "<=",
                [ExpressionType.AndAlso] = "AND",
                [ExpressionType.OrElse] = "OR",
                [ExpressionType.ExclusiveOr] = "XOR"
            };

        private static readonly IReadOnlyDictionary<ExpressionType, string> AltOperators
            = new Dictionary<ExpressionType, string>()
            {
                [ExpressionType.Equal] = "IS",
                [ExpressionType.NotEqual] = "IS NOT"
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