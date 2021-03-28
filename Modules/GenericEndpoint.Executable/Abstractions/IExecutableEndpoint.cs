namespace SpaceEngineers.Core.GenericEndpoint.Executable.Abstractions
{
    using System.Threading.Tasks;
    using AutoWiring.Api.Abstractions;
    using GenericEndpoint;

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