namespace SpaceEngineers.Core.DataAccess.Api.Abstractions
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IUniqueIdentified<TKey>
    {
        /// <summary>
        /// Version
        /// </summary>
        ulong Version { get; }
    }
}