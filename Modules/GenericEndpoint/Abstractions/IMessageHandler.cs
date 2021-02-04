namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Message handler abstraction
    /// Implements reaction on incoming messages
    /// </summary>
    /// <typeparam name="TMessage">TMessage type-argument</typeparam>
    public interface IMessageHandler<in TMessage> : IResolvable
        where TMessage : IIntegrationMessage
    {
        /// <summary>
        /// Handle incoming message
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="context">Integration context</param>
        /// <returns>Ongoing handle task</returns>
        Task Handle(TMessage message, IIntegrationContext context);
    }
}