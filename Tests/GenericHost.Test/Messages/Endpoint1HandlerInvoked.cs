namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using System.Text.Json.Serialization;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(TestIdentity.Endpoint1)]
    [Feature(TestFeatures.Test)]
    internal record Endpoint1HandlerInvoked : IIntegrationEvent
    {
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public Endpoint1HandlerInvoked()
        {
            HandlerType = default!;
            EndpointIdentity = default!;
        }

        public Endpoint1HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }
    }
}