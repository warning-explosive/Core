namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IModelMigrator
    /// </summary>
    public interface IModelMigrator
    {
        /// <summary>
        /// Upgrade
        /// </summary>
        /// <param name="manualMigrations">Manual migrations</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task Upgrade(
            IReadOnlyCollection<IManualMigration> manualMigrations,
            CancellationToken token);
    }
}