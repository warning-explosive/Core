namespace SpaceEngineers.Core.Web.Api.Containers
{
    /// <summary>
    /// IDataContainersProvider
    /// </summary>
    public interface IDataContainersProvider
    {
        /// <summary>
        /// Converts entity to view entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <typeparam name="TEntity">TEntity type-argument</typeparam>
        /// <returns>ViewEntity</returns>
        ViewEntity ToViewEntity<TEntity>(TEntity entity);
    }
}