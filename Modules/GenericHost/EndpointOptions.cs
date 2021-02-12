namespace SpaceEngineers.Core.GenericHost
{
    using System.Reflection;
    using AutoRegistration;

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

        /// <summary>
        /// Endpoint assembly that limits assembly loading for endpoint's dependency container
        /// </summary>
        public Assembly? Assembly { get; set; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions? ContainerOptions { get; set; }
    }
}