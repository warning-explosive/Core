namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contract.Abstractions;

    /// <summary>
    /// Integration context
    /// Use to managing integration messages between endpoints
    /// </summary>
    public interface IIntegrationContext
    {
        /// <summary>
        /// Request data from target endpoint
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        Task Request<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage;

        /// <summary>
        /// Reply to initiator endpoint
        /// Must be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="reply">Integration reply</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing reply operation</returns>
        Task Reply<TQuery, TReply>(TQuery query, TReply reply, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage;
    }
}