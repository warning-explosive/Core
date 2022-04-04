namespace SpaceEngineers.Core.DataAccess.Orm.Host.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// Builds database model from the existing database
    /// </summary>
    public interface IDatabaseModelBuilder : IResolvable
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel(CancellationToken token);
    }
}