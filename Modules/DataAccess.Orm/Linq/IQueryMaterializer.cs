namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Transaction;

    /// <summary>
    /// IQueryMaterializerComposite
    /// </summary>
    public interface IQueryMaterializerComposite : IQueryMaterializer
    {
    }

    /// <summary>
    /// IQueryMaterializer
    /// </summary>
    public interface IQueryMaterializer
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token);

        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            IQuery query,
            Type type,
            CancellationToken token);
    }

    /// <summary>
    /// IQueryMaterializer
    /// </summary>
    /// <typeparam name="TQuery">TQuery type-argument</typeparam>
    public interface IQueryMaterializer<TQuery>
        where TQuery : IQuery
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            TQuery query,
            Type type,
            CancellationToken token);

        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="query">Query</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            TQuery query,
            Type type,
            CancellationToken token);
    }
}