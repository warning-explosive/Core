namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// IModelChangesSorter
    /// </summary>
    public interface IModelChangesSorter
    {
        /// <summary>
        /// Sorts source changes collection
        /// </summary>
        /// <param name="source">Changes collection</param>
        /// <returns>Ordered changes sequence</returns>
        IOrderedEnumerable<IModelChange> Sort(IEnumerable<IModelChange> source);
    }
}