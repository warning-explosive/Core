namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Transaction;

    /// <summary>
    /// ICommandMaterializerComposite
    /// </summary>
    public interface ICommandMaterializerComposite : ICommandMaterializer
    {
    }

    /// <summary>
    /// ICommandMaterializer
    /// </summary>
    public interface ICommandMaterializer
    {
        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        Task<object?> MaterializeScalar(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            Type type,
            CancellationToken token);

        /// <summary>
        /// Materializes query
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="type">Type</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<object?> Materialize(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
            Type type,
            CancellationToken token);
    }

    /// <summary>
    /// ICommandMaterializer
    /// </summary>
    /// <typeparam name="TCommand">TCommand type-argument</typeparam>
    public interface ICommandMaterializer<TCommand>
        where TCommand : ICommand
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
            TCommand query,
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
            TCommand query,
            Type type,
            CancellationToken token);
    }
}