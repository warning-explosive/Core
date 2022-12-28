namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using Expressions;
    using Linq;

    /// <summary>
    /// ISqlQueryTranslator
    /// </summary>
    public interface ISqlQueryTranslator
    {
        /// <summary>
        /// Translates sql expression to DB query
        /// </summary>
        /// <param name="expression">Sql expression</param>
        /// <returns>Query</returns>
        IQuery Translate(ISqlExpression expression);
    }
}