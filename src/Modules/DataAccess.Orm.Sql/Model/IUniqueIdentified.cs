namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Model
{
    /// <summary>
    /// IUniqueIdentified
    /// </summary>
    public interface IUniqueIdentified
    {
        /// <summary>
        /// Primary key
        /// </summary>
        object PrimaryKey { get; }
    }

    /// <summary>
    /// IUniqueIdentified
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IUniqueIdentified<TKey> : IUniqueIdentified
        where TKey : notnull
    {
        /// <summary>
        /// Primary key
        /// </summary>
        new TKey PrimaryKey { get; }
    }
}