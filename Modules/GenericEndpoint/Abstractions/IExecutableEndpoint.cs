namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
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
        /// Process message
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Ongoing operation</returns>
        Task ProcessMessage(IntegrationMessage message);
    }
}