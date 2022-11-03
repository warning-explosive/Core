namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;
    using Linq;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    /// <typeparam name="TExpression">TExpression type-argument</typeparam>
    public interface IIntermediateQueryTranslator<in TExpression>
        where TExpression : IIntermediateExpression
    {
        /// <summary>
        /// Translates intermediate expression to DB query
        /// </summary>
        /// <param name="intermediateExpression">Intermediate expression</param>
        /// <returns>Query</returns>
        IQuery Translate(TExpression intermediateExpression);
    }
}