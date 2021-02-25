namespace SpaceEngineers.Core.GenericEndpoint
{
    /// <summary>
    /// Endpoint runtime info
    /// </summary>
    public class EndpointRuntimeInfo
    {
        /// <summary> .cctor </summary>
        /// <param name="endpointIdentity">Endpoint identity</param>
        /// <param name="message">Integration message, processing initiator</param>
        public EndpointRuntimeInfo(EndpointIdentity endpointIdentity, IntegrationMessage message)
        {
            EndpointIdentity = endpointIdentity;
            Message = message;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity EndpointIdentity { get; }

        /// <summary>
        /// Integration message, processing initiator
        /// </summary>
        public IntegrationMessage Message { get; }
    }
}