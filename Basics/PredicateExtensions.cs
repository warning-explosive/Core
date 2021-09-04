namespace SpaceEngineers.Core.Basics
{
    using System;
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
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
        }
    }
}