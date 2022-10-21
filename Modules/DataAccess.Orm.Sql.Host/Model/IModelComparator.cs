namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Host.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// IModelComparator
    /// </summary>
    public interface IModelComparator
    {
        /// <summary>
        /// Extracts diff between actual model and expected model
        /// </summary>
        /// <param name="actualModel">Actual model</param>
        /// <param name="expectedModel">Expected model</param>
        /// <returns>Diff</returns>
        public IEnumerable<IModelChange> ExtractDiff(
            DatabaseNode? actualModel,
            DatabaseNode? expectedModel);
    }
}