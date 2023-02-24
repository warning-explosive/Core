namespace SpaceEngineers.Core.DataAccess.Orm.Host.Abstractions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Linq;

    /// <summary>
    /// IMigration
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Migration's unique name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If returns true migration will be executed during startup process despite the fact that it could be applied earlier
        /// </summary>
        public bool ApplyEveryTime { get; }

        /// <summary>
        /// Builds and invokes a migration commands
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IReadOnlyCollection<ICommand>> InvokeCommands(CancellationToken token);
    }
}