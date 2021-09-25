namespace SpaceEngineers.Core.DataAccess.Api.Model
{
    /// <summary>
    /// IUniqueIdentified
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface IUniqueIdentified<TKey>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        TKey PrimaryKey { get; }
    }
}