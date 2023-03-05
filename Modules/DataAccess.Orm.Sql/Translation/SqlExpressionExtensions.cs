namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
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
        /// Replaces join expressions
        /// </summary>
        /// <param name="expression">ISqlExpression</param>
        /// <param name="joinExpression">JoinExpression</param>
        /// <param name="applyNaming">Apply naming</param>
        /// <returns>ISqlExpression with replaced join expressions</returns>
        public static ISqlExpression ReplaceJoinExpressions(
            this ISqlExpression expression,
            JoinExpression joinExpression,
            bool applyNaming)
        {
            return new ReplaceJoinExpressionsVisitor(joinExpression, applyNaming).Visit(expression);
        }
    }
}