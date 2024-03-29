namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    public interface IDatabaseEntity : IUniqueIdentified
    {
        /// <summary>
        /// Version
        /// </summary>
        public long Version { get; set; }
    }

    /// <summary>
    /// IDatabaseEntity
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IDatabaseEntity<TKey> : IDatabaseEntity, IUniqueIdentified<TKey>
        where TKey : notnull
    {
    }
}