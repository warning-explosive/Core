namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System.Collections.Generic;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    internal interface IEndpointInstanceSelectionBehavior
    {
        EndpointIdentity SelectInstance(
            IntegrationMessage message,
            IReadOnlyCollection<EndpointIdentity> endpoints);
    }
}