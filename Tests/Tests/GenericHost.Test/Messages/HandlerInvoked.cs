namespace SpaceEngineers.Core.GenericHost.Test.Messages
{
    using System;
    using System.Text.Json.Serialization;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Contract.Attributes;

    [OwnedBy(nameof(EndpointIdentity))]
    [Feature(TestFeatures.Test)]
    internal record HandlerInvoked : IIntegrationEvent
    {
        [JsonConstructor]
        [Obsolete("serialization constructor")]
        public HandlerInvoked()
        {
            HandlerType = default!;
            EndpointIdentity = default!;
        }

        public HandlerInvoked(Type handlerType, EndpointIdentity endpointIdentity)
        {
            HandlerType = handlerType;
            EndpointIdentity = endpointIdentity;
        }

        public Type HandlerType { get; init; }

        public EndpointIdentity EndpointIdentity { get; init; }
    }
}