namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Views
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// ISqlViewQueryProvider
    /// </summary>
    /// <typeparam name="TView">TView type-argument</typeparam>
    /// <typeparam name="TKey">TKey type-argument</typeparam>
    public interface ISqlViewQueryProvider<TView, TKey> : IResolvable
        where TView : ISqlView<TKey>
    {
        /// <summary>
        /// Gets view query
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        public Task<string> GetQuery(CancellationToken token);
    }
}