namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;

    /// <summary>
    /// IMethodCallExpressionTranslator
    /// </summary>
    public interface ILinqExpressionVisitor
    {
        /// <summary>
        /// Translates sql expression
        /// </summary>
        /// <param name="visitor">Visitor</param>
        /// <param name="context">TranslationContext</param>
        /// <param name="expression">Expression</param>
        /// <returns>Recognition result</returns>
        bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression);
    }
}