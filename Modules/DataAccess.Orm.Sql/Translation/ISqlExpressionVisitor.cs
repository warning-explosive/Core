namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;

    /// <summary>
    /// ISqlExpression
    /// </summary>
    public interface ISqlExpressionVisitor
    {
        /// <summary>
        /// Visit expression
        /// </summary>
        /// <param name="expression">Source expression</param>
        /// <returns>Modified expression</returns>
        ISqlExpression Visit(ISqlExpression expression);
    }
}