namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Api.Persisting;
    using Core.DataAccess.Api.Reading;
    using Core.DataAccess.Api.Transaction;
    using CrossCuttingConcerns.Json;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class EventStore : IEventStore,
                                IResolvable<IEventStore>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _jsonSerializer;

        public EventStore(
            IDatabaseContext databaseContext,
            IJsonSerializer jsonSerializer)
        {
            _databaseContext = databaseContext;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<TAggregate?> Get<TAggregate>(
            Guid aggregateId,
            DateTime timestamp,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            var databaseDomainEvents = await _databaseContext
               .Read<DatabaseDomainEvent>()
               .All()
               .Where(domainEvent => domainEvent.AggregateId == aggregateId
                                  && domainEvent.Timestamp <= timestamp)
               .OrderBy(domainEvent => domainEvent.Timestamp)
               .ToListAsync(token)
               .ConfigureAwait(false);

            var domainEvents = databaseDomainEvents
               .Select(domainEvent => _jsonSerializer.DeserializeObject(domainEvent.SerializedEvent, domainEvent.EventType))
               .OfType<IDomainEvent<TAggregate>>()
               .ToArray();

            if (domainEvents.Any())
            {
                return (TAggregate)typeof(TAggregate)
                   .GetConstructor(new[] { typeof(IEnumerable<IDomainEvent<TAggregate>>) })
                  !.Invoke(new object[] { domainEvents });
            }

            return default;
        }

        public Task Append<TAggregate, TEvent>(
            TEvent domainEvent,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent
        {
            var databaseDomainEvent = new DatabaseDomainEvent(
                Guid.NewGuid(),
                domainEvent.AggregateId,
                domainEvent.Index,
                domainEvent.Timestamp,
                domainEvent.GetType(),
                _jsonSerializer.SerializeObject(domainEvent));

            return _databaseContext
               .Write<DatabaseDomainEvent>()
               .Insert(new[] { databaseDomainEvent }, EnInsertBehavior.Default, token);
        }
    }
}