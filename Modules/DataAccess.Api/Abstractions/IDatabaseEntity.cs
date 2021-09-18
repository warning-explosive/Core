namespace SpaceEngineers.Core.DataAccess.Api.Abstractions
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IUniqueIdentified<TKey>
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