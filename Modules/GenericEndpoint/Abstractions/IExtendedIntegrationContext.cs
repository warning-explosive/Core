namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IExtendedIntegrationContext abstraction
    /// </summary>
    public interface IExtendedIntegrationContext : IIntegrationContext
    {
        /// <summary>
        /// Retry integration message processing
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <typeparam name="TMessage">TMessage type-argument</typeparam>
        /// <returns>Ongoing retry operation</returns>
        Task Retry<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage;
    }
}