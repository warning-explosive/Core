namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using Contract;
    using GenericDomain.Api.Abstractions;
    using Messaging;

    internal class InboxAggregateSpecification : IAggregateSpecification
    {
        public InboxAggregateSpecification(
            IntegrationMessage message,
            EndpointIdentity endpointIdentity)
        {
            Message = message;
            EndpointIdentity = endpointIdentity;
        }

        public IntegrationMessage Message { get; }

        public EndpointIdentity EndpointIdentity { get; }
    }
}