namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IModelChangesExtractor
    /// </summary>
    public interface IModelChangesExtractor
    {
        /// <summary>
        /// Extracts model changes
        /// </summary>
        /// <param name="databaseEntities">Database entities</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task<IReadOnlyCollection<IModelChange>> ExtractChanges(
            IReadOnlyCollection<Type> databaseEntities,
            CancellationToken token);
    }
}