namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Model;

    /// <summary>
    /// IModelMigrationsExecutor
    /// </summary>
    public interface IModelMigrationsExecutor : IResolvable
    {
        /// <summary>
        /// Executes manual migrations
        /// </summary>
        /// <param name="manualMigrations">Manual migrations</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task ExecuteManualMigrations(
            IReadOnlyCollection<IManualMigration> manualMigrations,
            CancellationToken token);

        /// <summary>
        /// Executes automatic migrations
        /// </summary>
        /// <param name="modelChanges">Model changes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task ExecuteAutoMigrations(
            IReadOnlyCollection<IModelChange> modelChanges,
            CancellationToken token);
    }
}