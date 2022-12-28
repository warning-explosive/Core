namespace SpaceEngineers.Core.GenericDomain.EventSourcing.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Json;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Reading;
    using DataAccess.Api.Transaction;

    [Component(EnLifestyle.Scoped)]
    internal class SqlEventStore : IEventStore,
                                   IResolvable<IEventStore>
    {
        private readonly IDatabaseContext _databaseContext;
        private readonly IJsonSerializer _jsonSerializer;

        public SqlEventStore(
            IDatabaseContext databaseContext,
            IJsonSerializer jsonSerializer)
        {
            _databaseContext = databaseContext;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            DateTime timestamp,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            var domainEvents = await GetEvents<TAggregate>(aggregateId, timestamp, token).ConfigureAwait(false);

            return domainEvents.Any()
                ? (TAggregate)typeof(TAggregate)
                   .GetConstructor(new[] { typeof(IDomainEvent<TAggregate>[]) })
                  !.Invoke(new object[] { domainEvents })
                : default;
        }

        public async Task<IReadOnlyCollection<IDomainEvent<TAggregate>>> GetEvents<TAggregate>(
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
               .OrderBy(domainEvent => domainEvent.Index)
               .ToListAsync(token)
               .ConfigureAwait(false);

            return databaseDomainEvents
               .Select(domainEvent => _jsonSerializer.DeserializeObject(domainEvent.SerializedEvent, domainEvent.EventType))
               .OfType<IDomainEvent<TAggregate>>()
               .ToArray();
        }

        public Task Append<TAggregate, TEvent>(
            TEvent domainEvent,
            DomainEventDetails details,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent<TAggregate>
        {
            var databaseDomainEvent = new DatabaseDomainEvent(
                Guid.NewGuid(),
                details.AggregateId,
                details.Index,
                details.Timestamp,
                domainEvent.GetType(),
                _jsonSerializer.SerializeObject(domainEvent));

            return _databaseContext.Insert<DatabaseDomainEvent>(new[] { databaseDomainEvent }, EnInsertBehavior.Default, token);
        }
    }
}