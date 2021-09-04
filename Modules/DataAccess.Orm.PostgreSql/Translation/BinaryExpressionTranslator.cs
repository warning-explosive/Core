namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Translation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Linq.Abstractions;
    using Linq.Internals;
    using BinaryExpression = Linq.Expressions.BinaryExpression;
    using ConstantExpression = Linq.Expressions.ConstantExpression;

    [Component(EnLifestyle.Singleton)]
    internal class BinaryExpressionTranslator : IExpressionTranslator<BinaryExpression>
    {
        private static readonly IReadOnlyDictionary<ExpressionType, string> FunctionalOperators
            = new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Coalesce] = "COALESCE"
            };

        private static readonly IReadOnlyDictionary<ExpressionType, string> Operators
            = new Dictionary<ExpressionType, string>
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
            = new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Equal] = "IS",
                [ExpressionType.NotEqual] = "IS NOT"
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