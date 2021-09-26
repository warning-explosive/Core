namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IDatabaseModelMigrator
    /// </summary>
    public interface IDatabaseModelMigrator : IResolvable
    {
        /// <summary>
        /// Migrate
        /// </summary>
        /// <param name="modelChanges">Model changes</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task Migrate(IReadOnlyCollection<IDatabaseModelChange> modelChanges, CancellationToken token);
    }
}