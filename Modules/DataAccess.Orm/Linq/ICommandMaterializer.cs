namespace SpaceEngineers.Core.DataAccess.Orm.Linq
{
    using System.Collections.Generic;
    using System.Threading;
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
        /// Materializes command
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            ICommand command,
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
        /// Materializes command
        /// </summary>
        /// <param name="transaction">IAdvancedDatabaseTransaction</param>
        /// <param name="command">Command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="T">T type-argument</typeparam>
        /// <returns>Ongoing materialization operation</returns>
        IAsyncEnumerable<T> Materialize<T>(
            IAdvancedDatabaseTransaction transaction,
            TCommand command,
            CancellationToken token);
    }
}