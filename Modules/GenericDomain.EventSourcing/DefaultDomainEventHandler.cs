namespace SpaceEngineers.Core.GenericDomain.EventSourcing
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Scoped)]
    internal class DefaultDomainEventHandler<TAggregate, TEvent> : IDomainEventHandler<TAggregate, TEvent>,
                                                                   IResolvable<IDomainEventHandler<TAggregate, TEvent>>
        where TAggregate : class, IAggregate<TAggregate>
        where TEvent : class, IDomainEvent<TAggregate>
    {
        private readonly IEventStore _eventStore;

        public DefaultDomainEventHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public Task Handle(DomainEventArgs<TEvent> args, CancellationToken token)
        {
            return _eventStore.Append(args, token);
        }
    }
}