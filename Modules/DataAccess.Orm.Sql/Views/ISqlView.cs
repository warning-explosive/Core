namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Views
{
    using Api.DatabaseEntity;

    /// <summary>
    /// ISqlView
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlView<TKey> : IUniqueIdentified<TKey>
    {
    }
}