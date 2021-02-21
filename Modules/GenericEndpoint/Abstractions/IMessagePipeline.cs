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
        /// <param name="message">Incoming integration message</param>
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing handle task</returns>
        Task Process(IntegrationMessage message, IExtendedIntegrationContext context, CancellationToken token);
    }
}