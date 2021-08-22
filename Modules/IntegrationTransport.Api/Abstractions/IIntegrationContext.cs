namespace SpaceEngineers.Core.IntegrationTransport.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using GenericEndpoint.Contract.Abstractions;

    /// <summary>
    /// Integration context
    /// Use to produce integration messages outside of endpoint scope
    /// </summary>
    public interface IIntegrationContext : IResolvable
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
        /// Request data with RPC pattern
        /// </summary>
        /// <param name="query">Integration query</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TQuery">TQuery type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing remote procedure call</returns>
        Task<TReply> RpcRequest<TQuery, TReply>(TQuery query, CancellationToken token)
            where TQuery : IIntegrationQuery<TReply>
            where TReply : IIntegrationMessage;
    }
}