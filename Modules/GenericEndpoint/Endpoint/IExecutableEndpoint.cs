namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
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
        /// <returns>Ongoing operation</returns>
        Task ExecuteMessageHandlers(IntegrationMessage message);
    }
}