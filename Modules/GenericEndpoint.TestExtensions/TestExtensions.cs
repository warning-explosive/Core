namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using Api.Abstractions;
    using Contract.Abstractions;

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
    }
}