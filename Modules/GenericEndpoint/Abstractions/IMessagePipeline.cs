namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// IMessagePipeline abstraction
    /// </summary>
    public interface IMessagePipeline : IResolvable
    {
        /// <summary>
        /// Handle incoming message
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Ongoing handle task</returns>
        Task Process<TMessage>(TMessage message, IExtendedIntegrationContext context, CancellationToken token)
            where TMessage : IIntegrationMessage;
    }
}