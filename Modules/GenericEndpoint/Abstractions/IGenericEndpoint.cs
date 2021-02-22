namespace SpaceEngineers.Core.GenericEndpoint.Abstractions
{
    using AutoWiringApi.Abstractions;
    using GenericEndpoint;

    /// <summary>
    /// Generic endpoint abstraction
    /// </summary>
    public interface IGenericEndpoint : IResolvable
    {
        /// <summary>
        /// Endpoint identity
        /// </summary>
        EndpointIdentity Identity { get; }

        /// <summary>
        /// Integration type provider
        /// </summary>
        IIntegrationTypeProvider IntegrationTypeProvider { get; }
    }
}