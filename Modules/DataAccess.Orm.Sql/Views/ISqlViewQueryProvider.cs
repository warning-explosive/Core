namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Views
{
    /// <summary>
    /// ISqlViewQueryProvider
    /// </summary>
    /// <typeparam name="TView">TView type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlViewQueryProvider<TView, TKey>
        where TView : ISqlView<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// Gets view query
        /// </summary>
        /// <returns>Ongoing operation</returns>
        public string GetQuery();
    }
}