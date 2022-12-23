namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    [Feature(nameof(Test))]
    internal record Endpoint1HandlerInvoked : IIntegrationEvent
    {
        public Endpoint1HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }
    }
}