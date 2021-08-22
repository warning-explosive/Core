namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Abstractions
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <summary>
    /// IEndpointInstanceSelectionBehavior
    /// </summary>
    public interface IEndpointInstanceSelectionBehavior : IResolvable
    {
        /// <summary>
        /// Select instance in one logical group
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="endpoints">Endpoint instances</param>
        /// <returns>Only one instance</returns>
        EndpointIdentity SelectInstance(
            IntegrationMessage message,
            IReadOnlyCollection<EndpointIdentity> endpoints);
    }
}