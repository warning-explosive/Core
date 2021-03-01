namespace SpaceEngineers.Core.GenericEndpoint.TestExtensions
{
    using Abstractions;
    using Contract.Abstractions;

    /// <summary>
    /// Test extensions for GenericEndpoint assembly
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Incoming test message
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