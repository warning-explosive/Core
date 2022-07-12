namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        /*TODO: #132 - historical entities*/
        /*TODO: #133 - optimistic concurrency control*/
        /*/// <summary>
        /// Version
        /// </summary>
        public long Version { get; }*/
    }
}