namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IExecutableEndpoint
    /// </summary>
    public interface IExecutableEndpoint
    {
        /// <summary>
        /// Executes message handler
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Ongoing operation</returns>
        Task ExecuteMessageHandler(IntegrationMessage message);
    }
}