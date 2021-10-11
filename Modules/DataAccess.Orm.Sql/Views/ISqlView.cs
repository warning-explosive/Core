namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Views
{
    using Api.Model;

    /// <summary>
    /// ISqlView
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlView<TKey> : IUniqueIdentified<TKey>
        where TKey : notnull
    {
    }
}