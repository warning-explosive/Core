namespace SpaceEngineers.Core.GenericEndpoint.Api.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using Contract.Abstractions;

    /// <summary>
    /// Message handler abstraction
    /// Implements reaction on incoming messages
    /// </summary>
    /// <typeparam name="TMessage">TMessage type-argument</typeparam>
    public interface IMessageHandler<TMessage> : IResolvable
        where TMessage : IIntegrationMessage
    {
        /// <summary>
        /// Handle incoming message
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing handle task</returns>
        Task Handle(
            TMessage message,
            IIntegrationContext context,
            CancellationToken token);
    }
}