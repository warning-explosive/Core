namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using System.Text.Json.Serialization;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint2)]
    [Feature(TestFeatures.Test)]
    internal record Endpoint2HandlerInvoked : IIntegrationEvent
    {
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public Endpoint2HandlerInvoked()
        {
            HandlerType = default!;
            EndpointIdentity = default!;
        }

        public Endpoint2HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }
    }
}