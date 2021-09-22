namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Deduplication
{
    using Contract;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class InboxMessageReceived : IDomainEvent
    {
        public InboxMessageReceived(IntegrationMessage message, EndpointIdentity endpointIdentity)
        {
            Message = message;
            EndpointIdentity = endpointIdentity;
        }

        public IntegrationMessage Message { get; }

        public EndpointIdentity EndpointIdentity { get; }
    }
}