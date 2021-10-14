namespace SpaceEngineers.Core.DataAccess.Orm.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IModelChangesSorter
    /// </summary>
    public interface IModelChangesSorter : IResolvable
    {
        /// <summary>
        /// Sorts source changes collection
        /// </summary>
        /// <param name="source">Changes collection</param>
        /// <returns>Ordered changes sequence</returns>
        IOrderedEnumerable<IModelChange> Sort(IEnumerable<IModelChange> source);
    }
}