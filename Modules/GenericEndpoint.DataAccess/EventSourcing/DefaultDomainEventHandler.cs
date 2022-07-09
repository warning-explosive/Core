namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class DefaultDomainEventHandler<TEvent> : IDomainEventHandler<TEvent>,
                                                       IResolvable<IDomainEventHandler<TEvent>>
        where TEvent : IDomainEvent
    {
        private readonly IEventStore _eventStore;

        public DefaultDomainEventHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public Task Handle(TEvent domainEvent, CancellationToken token)
        {
            var aggregateType = typeof(TEvent)
               .ExtractGenericArgumentsAt(typeof(IDomainEvent<>))
               .Single();

            return this
               .CallMethod(nameof(Append))
               .WithTypeArgument(aggregateType)
               .WithArguments(domainEvent, token)
               .Invoke<Task>();
        }

        private Task Append<TAggregate>(TEvent domainEvent, CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            return _eventStore.Append<TAggregate, TEvent>(domainEvent, token);
        }
    }
}