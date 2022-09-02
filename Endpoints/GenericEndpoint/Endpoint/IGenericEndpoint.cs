namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Threading.Tasks;
    using Messaging;

    /// <summary>
    /// IGenericEndpoint
    /// </summary>
    public interface IGenericEndpoint
    {
        /// <summary>
        /// Executes message handler
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <returns>Ongoing operation</returns>
        Task ExecuteMessageHandler(IntegrationMessage message);
    }
}