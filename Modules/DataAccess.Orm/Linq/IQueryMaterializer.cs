namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IQueryMaterializer
    /// </summary>
    /// <typeparam name="TQuery">TQuery type-argument</typeparam>
    /// <typeparam name="TItem">TItem type-argument</typeparam>
    public interface IQueryMaterializer<TQuery, TItem> : IResolvable
        where TQuery : IQuery
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        Task<TItem> MaterializeScalar(TQuery query, CancellationToken token);

        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<TItem> Materialize(TQuery query, CancellationToken token);
    }
}