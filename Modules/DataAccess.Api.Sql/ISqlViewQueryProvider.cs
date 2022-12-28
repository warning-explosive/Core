namespace SpaceEngineers.Core.DataAccess.Api.Sql
{
    using System;

    /// <summary>
    /// ISqlViewQueryProvider
    /// </summary>
    public interface ISqlViewQueryProviderComposite
    {
        /// <summary>
        /// Gets view query
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Ongoing operation</returns>
        public string GetQuery(Type type);
    }

    /// <summary>
    /// ISqlViewQueryProvider
    /// </summary>
    public interface ISqlViewQueryProvider
    {
        /// <summary>
        /// Gets view query
        /// </summary>
        /// <returns>Ongoing operation</returns>
        public string GetQuery();
    }

    /// <summary>
    /// ISqlViewQueryProvider
    /// </summary>
    /// <typeparam name="TView">TView type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlViewQueryProvider<TView, TKey> : ISqlViewQueryProvider
        where TView : ISqlView<TKey>
        where TKey : notnull
    {
    }
}