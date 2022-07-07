namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Transient)]
    internal class DomainEventMessageHandler<TEvent> : IMessageHandler<CaptureDomainEvent<TEvent>>,
                                                       IResolvable<IMessageHandler<CaptureDomainEvent<TEvent>>>
        where TEvent : IDomainEvent
    {
        private readonly IDomainEventHandler<TEvent> _domainEventHandler;

        public DomainEventMessageHandler(IDomainEventHandler<TEvent> domainEventHandler)
        {
            _domainEventHandler = domainEventHandler;
        }

        public Task Handle(CaptureDomainEvent<TEvent> message, CancellationToken token)
        {
            // TODO: #172 - test it
            return _domainEventHandler.Handle(message.Event, token);
        }
    }
}