namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    [Feature(TestFeatures.Test)]
    internal record Endpoint2HandlerInvoked : IIntegrationEvent
    {
        public Endpoint2HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }
    }
}