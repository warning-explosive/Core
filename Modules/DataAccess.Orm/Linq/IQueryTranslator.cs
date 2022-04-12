namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq.Expressions;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    public interface IQueryTranslator
    {
        /// <summary>
        /// Translates linq expression to DB query
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>Query</returns>
        IQuery Translate(Expression expression);
    }
}