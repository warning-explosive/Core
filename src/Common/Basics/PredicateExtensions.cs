namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// PredicateExtensions
    /// </summary>
    public static class PredicateExtensions
    {
        /// <summary>
        /// Not
        /// </summary>
        /// <param name="function">Function</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Negative function</returns>
        public static Func<T, bool> Not<T>(this Func<T, bool> function)
        {
            return input => !function.Invoke(input);
        }

        /// <summary>
        /// Not
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Negative expression</returns>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var parameter = expression.Parameters.Single();
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), parameter);
        }

        /// <summary>
        /// And
        /// </summary>
        /// <param name="left">Left expression</param>
        /// <param name="right">Right expression</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>And expression</returns>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = left.Parameters.Single();

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    left.Body.ReplaceParameter(parameter),
                    right.Body.ReplaceParameter(parameter)),
                parameter);
        }

        /// <summary>
        /// Or
        /// </summary>
        /// <param name="left">Left expression</param>
        /// <param name="right">Right expression</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Or expression</returns>
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            var parameter = left.Parameters.Single();

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(
                    left.Body.ReplaceParameter(parameter),
                    right.Body.ReplaceParameter(parameter)),
                parameter);
        }

        /// <summary>
        /// Replaces parameter with specified one
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <param name="parameterExpression">Replacement</param>
        /// <returns>Source expression with replaced parameter</returns>
        public static Expression ReplaceParameter(
            this Expression expression,
            ParameterExpression parameterExpression)
        {
            return ReplaceParameterVisitor.Replace(expression, parameterExpression);
        }

        /// <summary>
        /// Unwraps Quote, Convert, ConvertChecked unary expressions
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <returns>Source expression without unary expressions</returns>
        public static Expression UnwrapUnaryExpression(
            this Expression expression)
        {
            var expressionTypes = new[]
            {
                ExpressionType.Quote,
                ExpressionType.Convert,
                ExpressionType.ConvertChecked
            };

            return UnwrapUnaryExpressionVisitor.Unwrap(expression, expressionTypes);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _replacement;

            private ReplaceParameterVisitor(ParameterExpression replacement)
            {
                _replacement = replacement;
            }

            public static Expression Replace(Expression expression, ParameterExpression replacement)
            {
                return new ReplaceParameterVisitor(replacement).Visit(expression);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _replacement;
            }
        }

        private class UnwrapUnaryExpressionVisitor : ExpressionVisitor
        {
            private readonly ExpressionType[] _expressionTypes;

            private UnwrapUnaryExpressionVisitor(ExpressionType[] expressionTypes)
            {
                _expressionTypes = expressionTypes;
            }

            public static Expression Unwrap(Expression expression, ExpressionType[] expressionTypes)
            {
                return new UnwrapUnaryExpressionVisitor(expressionTypes).Visit(expression);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                return _expressionTypes.Contains(node.NodeType)
                    ? node.Operand
                    : base.VisitUnary(node);
            }
        }
    }
}