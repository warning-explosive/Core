namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using Api.Abstractions;
    using Contract.Abstractions;

    /// <summary>
    /// MessageHandlerTestExtensions
    /// </summary>
    public static class MessageHandlerTestExtensions
    {
        /// <summary>
        /// Tests handler reaction on incoming message in isolation
        /// </summary>
        /// <param name="handler">Message handler</param>
        /// <param name="message">Incoming integration message</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>MessageHandlerTestBuilder</returns>
        public static MessageHandlerTestBuilder<TMessage> OnMessage<TMessage>(
            this IMessageHandler<TMessage> handler,
            TMessage message)
            where TMessage : IIntegrationMessage
        {
            return new MessageHandlerTestBuilder<TMessage>(message, handler);
        }
    }
}