namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using CompositionRoot;
    using Contract;

    /// <summary>
    /// Endpoint initialization options
    /// </summary>
    public class EndpointOptions
    {
        /// <summary> .cctor </summary>
        /// <param name="identity">EndpointIdentity</param>
        /// <param name="containerOptions">DependencyContainerOptions</param>
        internal EndpointOptions(
            EndpointIdentity identity,
            DependencyContainerOptions containerOptions)
        {
            Identity = identity;
            ContainerOptions = containerOptions;
        }

        /// <summary>
        /// EndpointIdentity
        /// </summary>
        public EndpointIdentity Identity { get; }

        /// <summary>
        /// Dependency container options
        /// </summary>
        public DependencyContainerOptions ContainerOptions { get; }
    }
}