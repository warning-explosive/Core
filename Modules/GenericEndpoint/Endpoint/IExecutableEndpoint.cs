namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// IExecutableEndpoint
    /// </summary>
    public interface IExecutableEndpoint : IResolvable
    {
        /// <summary>
        /// Executes message handlers
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing operation</returns>
        Task ExecuteMessageHandlers(IntegrationMessage message, CancellationToken token);
    }
}