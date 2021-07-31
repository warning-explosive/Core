namespace SpaceEngineers.Core.DataAccess.Orm.Linq.Abstractions
{
    using System.Linq.Expressions;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IQueryTranslator
    /// </summary>
    public interface IQueryTranslator : IResolvable
    {
        /// <summary>
        /// Translate linq query expression
        /// </summary>
        /// <param name="expression">Linq query expression</param>
        /// <returns>Query</returns>
        IQuery Translate(Expression expression);
    }
}