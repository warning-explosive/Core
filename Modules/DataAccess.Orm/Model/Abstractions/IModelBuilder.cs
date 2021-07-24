namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    /// <summary>
    /// IModelBuilder
    /// </summary>
    public interface IModelBuilder
    {
        /// <summary>
        /// Builds database model from the specified source
        /// </summary>
        /// <returns>Built model nodes</returns>
        DatabaseNode? BuildModel();
    }
}