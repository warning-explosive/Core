namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;

    /// <summary>
    /// ISqlExpressionTranslatorComposite
    /// </summary>
    public interface ISqlExpressionTranslatorComposite : ISqlExpressionTranslator
    {
    }

    /// <summary>
    /// ISqlExpressionTranslator
    /// </summary>
    public interface ISqlExpressionTranslator
    {
        /// <summary>
        /// Translates sql expression into DB query
        /// </summary>
        /// <param name="expression">Sql expression</param>
        /// <param name="depth">Depth</param>
        /// <returns>Translated expression</returns>
        string Translate(ISqlExpression expression, int depth);
    }

    /// <summary>
    /// IExpressionTranslator
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface ISqlExpressionTranslator<TExpression> : ISqlExpressionTranslator
        where TExpression : ISqlExpression
    {
        /// <summary>
        /// Translates sql expression into DB query
        /// </summary>
        /// <param name="expression">Sql expression</param>
        /// <param name="depth">Depth</param>
        /// <returns>Translated expression</returns>
        string Translate(TExpression expression, int depth);
    }
}