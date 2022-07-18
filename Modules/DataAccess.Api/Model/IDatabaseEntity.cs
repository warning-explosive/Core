namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// Version
        /// </summary>
        /*TODO: #132 - historical entities*/
        public long Version { get; }
    }
}