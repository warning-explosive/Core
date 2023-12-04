namespace SpaceEngineers.Core.GenericEndpoint.Api.Abstractions
{
    using System;
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
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        void Send<TCommand>(TCommand command)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Sends integration command to logical owner with delay
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="dueTime">Time that transport waits before deliver command</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        void Delay<TCommand>(TCommand command, TimeSpan dueTime)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Sends integration command to logical owner with delay
        /// </summary>
        /// <param name="command">Integration command</param>
        /// <param name="dateTime">DateTime that transport waits before deliver command</param>
        /// <typeparam name="TCommand">TCommand type-argument</typeparam>
        void Delay<TCommand>(TCommand command, DateTime dateTime)
            where TCommand : IIntegrationCommand;

        /// <summary>
        /// Publishes integration event to subscribers
        /// </summary>
        /// <param name="integrationEvent">Integration event</param>
        /// <typeparam name="TEvent">TEvent type-argument</typeparam>
        void Publish<TEvent>(TEvent integrationEvent)
            where TEvent : IIntegrationEvent;

        /// <summary>
        /// Requests data from target endpoint
        /// </summary>
        /// <param name="request">Integration request</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        void Request<TRequest, TReply>(TRequest request)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply;

        /// <summary>
        /// Replies to initiator endpoint
        /// Should be called within endpoint scope (in message handler)
        /// </summary>
        /// <param name="request">Integration request</param>
        /// <param name="reply">Integration reply</param>
        /// <typeparam name="TRequest">TRequest type-argument</typeparam>
        /// <typeparam name="TReply">TReply type-argument</typeparam>
        void Reply<TRequest, TReply>(TRequest request, TReply reply)
            where TRequest : IIntegrationRequest<TReply>
            where TReply : IIntegrationReply;
    }
}