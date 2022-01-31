namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IManualMigration
    /// </summary>
    public interface IManualMigration
    {
        /// <summary>
        /// Migration unique name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes manual migration
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task ExecuteManualMigration(CancellationToken token);
    }
}