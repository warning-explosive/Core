namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing
{
    using Contract;
    using Contract.Abstractions;
    using Contract.Attributes;
    using SpaceEngineers.Core.GenericDomain.Api.Abstractions;

    [OwnedBy(nameof(EndpointIdentity))]
    internal record CaptureDomainEvent<TAggregate, TEvent> : IIntegrationCommand
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