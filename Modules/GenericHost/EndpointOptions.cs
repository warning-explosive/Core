namespace SpaceEngineers.Core.GenericHost
{
    using System.Reflection;
    using Abstractions;
    using AutoRegistration;
    using GenericEndpoint;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">Endpoint identity</param>
        /// <param name="transport">Integration transport</param>
        public EndpointOptions(EndpointIdentity identity, IIntegrationTransport transport)
        {
            Identity = identity;
            Transport = transport;
        }

        /// <summary>
        /// Endpoint identity
        /// </summary>
        public EndpointIdentity Identity { get; }

        /// <summary>
        /// Endpoint assembly that limits assembly loading for endpoint's dependency container
        /// </summary>
        public Assembly? Assembly { get; set; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions? ContainerOptions { get; set; }

        /// <summary>
        /// Integration transport instance
        /// </summary>
        public IIntegrationTransport Transport { get; }
    }
}