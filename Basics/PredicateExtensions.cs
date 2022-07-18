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
            return new ReplaceParameterVisitor(parameterExpression).Visit(expression);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly Expression _replacement;

            public ReplaceParameterVisitor(Expression replacement)
            {
                _replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _replacement;
            }
        }
    }
}