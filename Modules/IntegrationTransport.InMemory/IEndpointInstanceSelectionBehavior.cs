namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System.Collections.Generic;
    using AutoRegistration.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    internal interface IEndpointInstanceSelectionBehavior : IResolvable
    {
        EndpointIdentity SelectInstance(
            IntegrationMessage message,
            IReadOnlyCollection<EndpointIdentity> endpoints);
    }
}