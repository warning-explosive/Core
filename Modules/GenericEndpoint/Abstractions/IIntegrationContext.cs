namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contract.Abstractions;

    /// <summary>
    /// Integration context
    /// Used for managing integration operations between endpoints
    /// </summary>
    public interface IIntegrationContext
    {
        /// <summary>
        /// Send integration command to logical owner
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Publish integration event to subscribers
        /// </summary>
        /// <param name="integrationEvent">Integration event</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>Ongoing publish operation</returns>
        Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent;

        /// <summary>
        /// Request data from target endpoint
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TResponse">TResponse type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        Task Request<TQuery, TResponse>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage;

        /// <summary>
        /// Reply to initiator endpoint
        /// Must be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="response">Integration response</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TResponse">TResponse type-argument</typeparam>
        /// <returns>Ongoing reply operation</returns>
        Task Reply<TQuery, TResponse>(TQuery query, TResponse response, CancellationToken token)
            where TQuery : IIntegrationQuery<TResponse>
            where TResponse : IIntegrationMessage;
    }
}