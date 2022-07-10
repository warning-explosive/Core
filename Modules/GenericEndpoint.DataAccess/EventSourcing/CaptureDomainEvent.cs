namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using Contract.Abstractions;
    using Contract.Attributes;
    using DatabaseModel;
    using GenericDomain.Api.Abstractions;

    [OwnedBy(nameof(EndpointIdentity))]
    internal class CaptureDomainEvent<TEvent> : IIntegrationCommand
        where TEvent : class, IDomainEvent
    {
        public CaptureDomainEvent(TEvent @event)
        {
            Event = @event;
        }

        public TEvent Event { get; init; }
    }
}