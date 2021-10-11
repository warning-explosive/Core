namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IUniqueIdentified<TKey>
        where TKey : notnull
    {
        /*
         * TODO: #133 - Versions, optimistic / pessimistic concurrency
         * TODO: #132 - historical entities
         * /// <summary>
         * /// Version
         * /// </summary>
         * ulong Version { get; }
         */
    }
}