namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Linq.Expressions;

    /// <summary>
    /// IMemberInfoTranslator
    /// </summary>
    public interface IUnknownExpressionTranslator
    {
        /// <summary>
        /// Translates sql expression
        /// </summary>
        /// <param name="context">TranslationContext</param>
        /// <param name="expression">Expression</param>
        /// <param name="visitor">Visitor</param>
        /// <returns>Recognition result</returns>
        bool TryTranslate(
            TranslationContext context,
            Expression expression,
            ExpressionVisitor visitor);
    }
}