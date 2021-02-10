namespace SpaceEngineers.Core.GenericHost
{
    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">Endpoint identity</param>
        public EndpointOptions(EndpointIdentity identity)
        {
            Identity = identity;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity Identity { get; }
    }
}