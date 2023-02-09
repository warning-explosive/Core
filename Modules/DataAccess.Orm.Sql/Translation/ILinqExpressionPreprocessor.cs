namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;

    /// <summary>
    /// ILinqExpressionPreprocessorComposite
    /// </summary>
    public interface ILinqExpressionPreprocessorComposite : ILinqExpressionPreprocessor
    {
    }

    /// <summary>
    /// ILinqExpressionPreprocessor
    /// </summary>
    public interface ILinqExpressionPreprocessor
    {
        /// <summary>
        /// Visits linq expression
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <returns>Returns modified expression</returns>
        Expression Visit(Expression expression);
    }
}