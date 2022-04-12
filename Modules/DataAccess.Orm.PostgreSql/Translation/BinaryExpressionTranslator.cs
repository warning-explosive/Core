namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Collections.Generic;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;
    using BinaryExpression = Sql.Translation.Expressions.BinaryExpression;
    using ConstantExpression = Sql.Translation.Expressions.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>,
                                                IResolvable<IExpressionTranslator<BinaryExpression>>
    {
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
                [BinaryOperator.Like] = "LIKE"
            };

        private static readonly IReadOnlyDictionary<BinaryOperator, string> AltOperators
            = new Dictionary<BinaryOperator, string>
            {
                [BinaryOperator.Equal] = "IS",
                [BinaryOperator.NotEqual] = "IS NOT"
            };

        private readonly IDependencyContainer _dependencyContainer;

        public BinaryExpressionTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(BinaryExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (FunctionalOperators.ContainsKey(expression.Operator))
            {
                sb.Append(FunctionalOperators[expression.Operator]);
                sb.Append('(');
                sb.Append(expression.Left.Translate(_dependencyContainer, depth));
                sb.Append(", ");
                sb.Append(expression.Right.Translate(_dependencyContainer, depth));
                sb.Append(')');
            }
            else
            {
                var map = (IsNullConstant(expression.Left) || IsNullConstant(expression.Right))
                          && AltOperators.ContainsKey(expression.Operator)
                    ? AltOperators
                    : Operators;

                sb.Append(expression.Left.Translate(_dependencyContainer, depth));
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

                sb.Append(expression.Right.Translate(_dependencyContainer, depth + 1));

                if (expression.Operator == BinaryOperator.Contains)
                {
                    sb.Append(')');
                }
            }

            return sb.ToString();
        }

        private static bool IsNullConstant(IIntermediateExpression expression)
        {
            return expression is ConstantExpression { Value: null };
        }
    }
}