namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
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
        /// <param name="token">Cancellation token</param>
        /// <returns>Query</returns>
        Task<IQuery> Translate(Expression expression, CancellationToken token);
    }
}