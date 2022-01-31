namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IModelMigrator
    /// </summary>
    public interface IModelMigrator : IResolvable
    {
        /// <summary>
        /// Executes manual migrations
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task ExecuteManualMigrations(CancellationToken token);

        /// <summary>
        /// Executes automatic migrations
        /// </summary>
        /// <param name="modelChanges">Model changes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task ExecuteAutoMigrations(IReadOnlyCollection<IModelChange> modelChanges, CancellationToken token);
    }
}