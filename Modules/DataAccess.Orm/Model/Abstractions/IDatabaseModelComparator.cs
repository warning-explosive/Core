namespace SpaceEngineers.Core.DataAccess.Orm.Model.Abstractions
{
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IModelComparator
    /// </summary>
    public interface IDatabaseModelComparator : IResolvable
    {
        /// <summary>
        /// Extracts diff between actual model and expected model
        /// </summary>
        /// <param name="actualModel">Actual model</param>
        /// <param name="expectedModel">Expected model</param>
        /// <returns>Diff</returns>
        public IEnumerable<IDatabaseModelChange> ExtractDiff(
            DatabaseNode? actualModel,
            DatabaseNode? expectedModel);
    }
}