namespace SpaceEngineers.Core.DataAccess.Orm.Host.Model
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IModelComparator
    /// </summary>
    public interface IModelComparator : IResolvable
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