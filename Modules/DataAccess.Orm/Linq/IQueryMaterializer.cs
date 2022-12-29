namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;

    /// <summary>
    /// IQueryMaterializer
    /// </summary>
    /// <typeparam name="TQuery">TQuery type-argument</typeparam>
    /// <typeparam name="TItem">TItem type-argument</typeparam>
    public interface IQueryMaterializer<TQuery, TItem>
        where TQuery : IQuery
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        Task<TItem> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            TQuery query,
            CancellationToken token);

        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<TItem> Materialize(
            IAdvancedDatabaseTransaction transaction,
            TQuery query,
            CancellationToken token);
    }
}