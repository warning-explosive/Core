namespace SpaceEngineers.Core.DataAccess.Orm.Abstractions
{
    using System.Linq;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IQueryBuilder
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IQueryBuilder<T> : IResolvable
    {
        /// <summary>
        /// Builds query from query object
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Database query and parameters object</returns>
        (string, object) BuildQuery(IQueryable<T> query);
    }
}