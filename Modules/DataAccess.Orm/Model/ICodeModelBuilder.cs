namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Builds database model from the existing code base
    /// </summary>
    public interface ICodeModelBuilder : IModelBuilder, IResolvable
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <param name="databaseEntities">Database entities</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel(Type[] databaseEntities, CancellationToken token);
    }
}