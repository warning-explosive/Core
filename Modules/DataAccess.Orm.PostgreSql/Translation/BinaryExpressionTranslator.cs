namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Sql.Translation;
    using Sql.Translation.Expressions;
    using Sql.Translation.Extensions;
    using BinaryExpression = Sql.Translation.Expressions.BinaryExpression;
    using ConstantExpression = Sql.Translation.Expressions.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>
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

        public async Task<string> Translate(BinaryExpression expression, int depth, CancellationToken token)
        {
            if (FunctionalOperators.ContainsKey(expression.Operator))
            {
                var sb = new StringBuilder();

                sb.Append(FunctionalOperators[expression.Operator]);
                sb.Append('(');
                sb.Append(await expression.Left.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
                sb.Append(", ");
                sb.Append(await expression.Right.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
                sb.Append(')');

                return sb.ToString();
            }

            var map = (IsNullConstant(expression.Left) || IsNullConstant(expression.Right))
                      && AltOperators.ContainsKey(expression.Operator)
                ? AltOperators
                : Operators;

            return string.Join(" ",
                await expression.Left.Translate(_dependencyContainer, depth, token).ConfigureAwait(false),
                map[expression.Operator],
                await expression.Right.Translate(_dependencyContainer, depth, token).ConfigureAwait(false));
        }

        private static bool IsNullConstant(IIntermediateExpression expression)
        {
            return expression is ConstantExpression { Value: null };
        }
    }
}