namespace SpaceEngineers.Core.Web.Api.Containers
{
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IDataContainersProvider
    /// </summary>
    public interface IDataContainersProvider : IResolvable
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