namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// IModelBuilder
    /// </summary>
    public interface IModelBuilder
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <returns>Built model nodes</returns>
        Task<DatabaseNode?> BuildModel();
    }
}