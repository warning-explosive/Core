namespace SpaceEngineers.Core.GenericHost.Abstractions
{
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;

    /// <summary>
    /// Generic endpoint abstraction
    /// </summary>
    public interface IGenericEndpoint
    {
        /// <summary>
        /// Endpoint identity
        /// </summary>
        EndpointIdentity Identity { get; }

        /// <summary>
        /// Integration types provider
        /// </summary>
        IIntegrationTypesProvider IntegrationTypesProvider { get; }
    }
}