namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IMigrationsExecutor
    /// </summary>
    public interface IMigrationsExecutor
    {
        /// <summary>
        /// Executes database model migrations
        /// </summary>
        /// <param name="migrations">Migrations</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task Migrate(
            IReadOnlyCollection<IMigration> migrations,
            CancellationToken token);
    }
}