namespace SpaceEngineers.Core.Basics
{
    using System.Linq.Expressions;

    /// <summary>
    /// ExpressionExtensions
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Invokes and replaces expression with constant
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <returns>Constant expression</returns>
        public static ConstantExpression CollapseConstantExpression(this Expression expression)
        {
            return Expression.Constant(Expression.Lambda(expression).Compile().DynamicInvoke());
        }
    }
}