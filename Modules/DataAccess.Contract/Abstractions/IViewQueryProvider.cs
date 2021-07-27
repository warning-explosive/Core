namespace SpaceEngineers.Core.DataAccess.Contract.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IViewQueryProvider
    /// </summary>
    /// <typeparam name="TView">TView type-argument</typeparam>
    public interface IViewQueryProvider<TView> : IResolvable
        where TView : IView
    {
        /// <summary>
        /// Gets view query
        /// </summary>
        /// <param name="token">Cancellation token</param>
        public Task<string> GetQuery(CancellationToken token);
    }
}