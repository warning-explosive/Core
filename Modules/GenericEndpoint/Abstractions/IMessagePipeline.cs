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
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing handle task</returns>
        Task Process(IExtendedIntegrationContext context, CancellationToken token);
    }
}