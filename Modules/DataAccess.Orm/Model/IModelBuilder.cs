namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IModelBuilder
    /// </summary>
    public interface IModelBuilder
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel(CancellationToken token);
    }
}