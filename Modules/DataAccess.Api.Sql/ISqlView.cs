namespace SpaceEngineers.Core.DataAccess.Api.Sql
{
    using Model;

    /// <summary>
    /// ISqlView
    /// </summary>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlView<TKey> : IUniqueIdentified<TKey>
        where TKey : notnull
    {
    }
}