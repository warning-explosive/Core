namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Expressions;
    using ParameterExpression = Expressions.ParameterExpression;

    /// <summary>
    /// ExpressionExtensions
    /// </summary>
    public static class SqlExpressionExtensions
    {
        /// <summary>
        /// Extracts ParameterExpressions
        /// </summary>
        /// <param name="expression">ISqlExpression</param>
        /// <returns>ParameterExpressions</returns>
        public static IReadOnlyDictionary<int, ParameterExpression> ExtractParameters(
            this ISqlExpression expression)
        {
            var extractor = new ExtractParametersVisitor();
            _ = extractor.Visit(expression);
            return extractor.Parameters;
        }

        /// <summary>
        /// Compacts ISqlExpression
        /// </summary>
        /// <param name="expression">ISqlExpression</param>
        /// <param name="projection">ProjectionExpression</param>
        /// <returns>Compacted ISqlExpression</returns>
        public static ISqlExpression CompactExpression(
            this ISqlExpression expression,
            ProjectionExpression projection)
        {
            return new CompactExpressionVisitor(projection).Visit(expression);
        }

        /// <summary>
        /// Replaces join bindings
        /// </summary>
        /// <param name="expression">ISqlExpression</param>
        /// <param name="joinExpression">JoinExpression</param>
        /// <param name="applyNaming">Apply naming</param>
        /// <returns>ISqlExpression with replaced join bindings</returns>
        public static ISqlExpression ReplaceJoinBindings(
            this ISqlExpression expression,
            JoinExpression joinExpression,
            bool applyNaming)
        {
            return new ReplaceJoinBindingsVisitor(joinExpression, applyNaming).Visit(expression);
        }

        /// <summary>
        /// Extracts members chain
        /// </summary>
        /// <param name="accessor">Accessor</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>Members chain</returns>
        public static PropertyInfo[] ExtractMembersChain<TEntity>(this Expression<Func<TEntity, object?>> accessor)
        {
            var visitor = new ExtractMembersChainExpressionVisitor();
            _ = visitor.Visit(accessor);
            return visitor.Chain;
        }
    }
}