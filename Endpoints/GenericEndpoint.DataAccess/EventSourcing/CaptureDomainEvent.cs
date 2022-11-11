namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using Contract.Abstractions;
    using Contract.Attributes;
    using Deduplication;
    using GenericDomain.Api.Abstractions;

    [OwnedBy(nameof(EndpointIdentity))]
    internal class CaptureDomainEvent<TAggregate, TEvent> : IIntegrationCommand
        where TAggregate : class, IAggregate<TAggregate>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        public CaptureDomainEvent(DomainEventArgs args)
        {
            Args = args;
        }

        public DomainEventArgs Args { get; init; }
    }
}