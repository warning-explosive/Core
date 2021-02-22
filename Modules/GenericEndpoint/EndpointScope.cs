namespace SpaceEngineers.Core.GenericEndpoint
{
    /// <summary>
    /// EndpointScope
    /// </summary>
    public class EndpointScope
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">Endpoint identity</param>
        /// <param name="initiatorMessage">Message handler initiator</param>
        public EndpointScope(EndpointIdentity identity, IntegrationMessage initiatorMessage)
        {
            Identity = identity;
            InitiatorMessage = initiatorMessage;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity Identity { get; }

        /// <summary>
        /// Message handler initiator
        /// </summary>
        public IntegrationMessage InitiatorMessage { get; }
    }
}