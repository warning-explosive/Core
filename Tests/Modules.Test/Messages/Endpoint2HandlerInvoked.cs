namespace SpaceEngineers.Core.Modules.Test.Messages
{
    using System;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    internal class Endpoint2HandlerInvoked : IIntegrationEvent
    {
        public Endpoint2HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; }

        public EndpointIdentity EndpointIdentity { get; }
    }
}