namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
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
        /// With message handler
        /// </summary>
        /// <param name="endpointBuilder">Endpoint builder</param>
        /// <typeparam name="THandler">THandler type-argument</typeparam>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>EndpointBuilder</returns>
        public static EndpointBuilder WithMessageHandler<THandler, TMessage>(this EndpointBuilder endpointBuilder)
            where THandler : IMessageHandler<TMessage>
            where TMessage : IIntegrationMessage
        {
            return endpointBuilder.ModifyContainerOptions(options => options.WithManualRegistrations(new MessageHandlerManualRegistration(typeof(THandler))));
        }
    }
}