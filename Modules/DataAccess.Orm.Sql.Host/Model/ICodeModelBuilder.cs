namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Builds database model from the existing code base
    /// </summary>
    public interface ICodeModelBuilder
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <param name="databaseEntities">Database entities</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel(IReadOnlyCollection<Type> databaseEntities, CancellationToken token);
    }
}