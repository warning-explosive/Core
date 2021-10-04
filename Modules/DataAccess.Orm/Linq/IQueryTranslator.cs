namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    public interface IQueryTranslator : IResolvable
    {
        /// <summary>
        /// Translates linq expression to DB query
        /// </summary>
        /// <param name="expression">Linq expression</param>
        /// <returns>Query</returns>
        IQuery Translate(Expression expression);
    }
}