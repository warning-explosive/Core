namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using Messaging;

    /// <summary>
    /// IExecutableEndpoint
    /// </summary>
    public interface IExecutableEndpoint : IResolvable
    {
        /// <summary>
        /// Invokes message handler
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Ongoing invoke operation</returns>
        Task InvokeMessageHandler(IntegrationMessage message);
    }
}