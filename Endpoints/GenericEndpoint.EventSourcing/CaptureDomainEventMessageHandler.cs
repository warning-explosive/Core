namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.GenericDomain.Api.Abstractions;
    using SpaceEngineers.Core.GenericDomain.EventSourcing;

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
            return _domainEventHandler.Handle(new DomainEventArgs<TEvent>(message.Args), token);
        }
    }
}