namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using System;
    using Api.Abstractions;
    using Contract.Abstractions;
    using Host;
    using Internals;

    /// <summary>
    /// TestExtensions
    /// </summary>
    public static class TestExtensions
    {
        /// <summary>
        /// Incoming test message
        /// </summary>
        /// <param name="handler">Message handler</param>
        /// <param name="message">Incoming integration message</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>TestMessageHandlerBuilder</returns>
        public static TestMessageHandlerBuilder<TMessage> OnMessage<TMessage>(
            this IMessageHandler<TMessage> handler,
            TMessage message)
            where TMessage : IIntegrationMessage
        {
            return new TestMessageHandlerBuilder<TMessage>(message, handler);
        }

        /// <summary>
        /// With message handlers
        /// </summary>
        /// <param name="endpointBuilder">Endpoint builder</param>
        /// <param name="types">Message handlers</param>
        /// <returns>EndpointBuilder</returns>
        public static EndpointBuilder WithMessageHandlers(this EndpointBuilder endpointBuilder, params Type[] types)
        {
            return endpointBuilder.ModifyContainerOptions(options => options.WithManualRegistration(new MessageHandlerManualRegistration(types)));
        }
    }
}