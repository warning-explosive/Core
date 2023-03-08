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
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Transaction;

    [Component(EnLifestyle.Scoped)]
    internal class SqlEventStore : IEventStore,
                                   IResolvable<IEventStore>
    {
        private readonly IDatabaseContext _databaseContext;

        public SqlEventStore(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
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
               .All<DatabaseDomainEvent>()
               .Where(domainEvent => domainEvent.AggregateId == aggregateId
                                  && domainEvent.Timestamp <= timestamp)
               .OrderBy(domainEvent => domainEvent.Index)
               .CachedExpression("29F74146-749E-454E-8F47-62A213DD44DA")
               .ToListAsync(token)
               .ConfigureAwait(false);

            return databaseDomainEvents
               .Select(domainEvent => domainEvent.DomainEvent)
               .Cast<IDomainEvent<TAggregate>>()
               .ToArray();
        }

        public Task Append(
            DomainEventArgs args,
            CancellationToken token)
        {
            return _databaseContext
                .Insert(new[] { BuildDatabaseDomainEvent(args) }, EnInsertBehavior.Default)
                .CachedExpression("FA2B0061-73A6-4CCA-B4D3-84D77357555A")
                .Invoke(token);
        }

        public Task Append(
            IReadOnlyCollection<DomainEventArgs> args,
            CancellationToken token)
        {
            return _databaseContext
                .Insert(args.Select(BuildDatabaseDomainEvent).ToArray(), EnInsertBehavior.Default)
                .CachedExpression($"{nameof(Append)}DomainEvents:{args.Count}")
                .Invoke(token);
        }

        private DatabaseDomainEvent BuildDatabaseDomainEvent(DomainEventArgs args)
        {
            return new DatabaseDomainEvent(
                Guid.NewGuid(),
                args.AggregateId,
                args.Index,
                args.Timestamp,
                args.DomainEvent);
        }
    }
}