namespace SpaceEngineers.Core.GenericEndpoint.Api.Abstractions
{
    using System;
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
        /// Sends integration command to logical owner
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Sends integration command to logical owner with delay
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="dueTime">Time that transport waits before deliver command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Delay<TCommand>(TCommand command, TimeSpan dueTime, CancellationToken token)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Sends integration command to logical owner with delay
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="dateTime">DateTime that transport waits before deliver command</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        /// <returns>Ongoing send operation</returns>
        Task Delay<TCommand>(TCommand command, DateTime dateTime, CancellationToken token)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Publishes integration event to subscribers
        /// </summary>
        /// <param name="integrationEvent">Integration event</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        /// <returns>Ongoing publish operation</returns>
        Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent;

        /// <summary>
        /// Requests data from target endpoint
        /// </summary>
        /// <param name="request">Integration request</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        Task Request<TRequest, TReply>(TRequest request, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply;

        /// <summary>
        /// Requests data from target endpoint in blocking manner
        /// </summary>
        /// <param name="request">Integration request</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing request operation</returns>
        Task<TReply> RpcRequest<TRequest, TReply>(TRequest request, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply;

        /// <summary>
        /// Replies to initiator endpoint
        /// Should be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="request">Integration request</param>
        /// <param name="reply">Integration reply</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        /// <returns>Ongoing reply operation</returns>
        Task Reply<TRequest, TReply>(TRequest request, TReply reply, CancellationToken token)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply;
    }
}