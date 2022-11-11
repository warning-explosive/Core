namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;

    [Component(EnLifestyle.Transient)]
    internal class CaptureDomainEventMessageHandler<TAggregate, TEvent> : IMessageHandler<CaptureDomainEvent<TAggregate, TEvent>>,
                                                                          IResolvable<IMessageHandler<CaptureDomainEvent<TAggregate, TEvent>>>
        where TAggregate : class, IAggregate<TAggregate>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        private readonly IDomainEventHandler<TAggregate, TEvent> _domainEventHandler;

        public CaptureDomainEventMessageHandler(IDomainEventHandler<TAggregate, TEvent> domainEventHandler)
        {
            _domainEventHandler = domainEventHandler;
        }

        public Task Handle(CaptureDomainEvent<TAggregate, TEvent> message, CancellationToken token)
        {
            return _domainEventHandler.Handle((TEvent)message.Args.DomainEvent, message.Args.Details, token);
        }
    }
}