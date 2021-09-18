namespace SpaceEngineers.Core.GenericEndpoint.Pipeline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;

    /// <summary>
    /// IMessagePipeline abstraction
    /// </summary>
    public interface IMessagePipeline : IResolvable
    {
        /// <summary>
        /// Handle incoming message
        /// </summary>
        /// <param name="producer">Message handler function</param>
        /// <param name="context">Integration context</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing handle task</returns>
        Task Process(
            Func<IAdvancedIntegrationContext, CancellationToken, Task> producer,
            IAdvancedIntegrationContext context,
            CancellationToken token);
    }
}