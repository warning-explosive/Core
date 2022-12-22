namespace SpaceEngineers.Core.GenericEndpoint.Messaging
{
    /// <summary>
    /// IIntegrationMessageHeaderProvider
    /// </summary>
    public interface IIntegrationMessageHeaderProvider
    {
        /// <summary>
        /// Fills message headers with context information
        /// </summary>
        /// <param name="generalMessage">General message</param>
        /// <param name="initiatorMessage">Initiator message</param>
        void WriteHeaders(IntegrationMessage generalMessage, IntegrationMessage? initiatorMessage);
    }
}