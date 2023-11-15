namespace SpaceEngineers.Core.GenericDomain.EventSourcing.Sql
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using DataAccess.Orm.Sql.Linq;
    using DataAccess.Orm.Sql.Transaction;

    [Component(EnLifestyle.Scoped)]
    internal class SqlEventStore : IEventStore,
                                   IResolvable<IEventStore>
    {
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> Cctors
            = new ConcurrentDictionary<Type, ConstructorInfo>();

        private readonly IDatabaseContext _databaseContext;

        public SqlEventStore(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            return GetAggregate<TAggregate>(aggregateId, _ => true, $"{nameof(GetAggregate)}:0A2D86C0-55C6-4AB6-A3C3-467363367DBC", token);
        }

        public Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            long version,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            return GetAggregate<TAggregate>(aggregateId, domainEvent => domainEvent.Index <= version, $"{nameof(GetAggregate)}ByVersion:0A2D86C0-55C6-4AB6-A3C3-467363367DBC", token);
        }

        public Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            DateTime timestamp,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            return GetAggregate<TAggregate>(aggregateId, domainEvent => domainEvent.Timestamp <= timestamp, $"{nameof(GetAggregate)}ByTimestamp:0A2D86C0-55C6-4AB6-A3C3-467363367DBC", token);
        }

        public Task Append(
            DomainEventArgs args,
            CancellationToken token)
        {
            return _databaseContext
                .Insert(new[] { BuildDatabaseDomainEvent(args) }, EnInsertBehavior.Default)
                .CachedExpression($"{nameof(Append)}DomainEvents:1:EC3B8AAD-265E-46F9-A088-ABE5178FAEA5")
                .Invoke(token);
        }

        public Task Append(
            IReadOnlyCollection<DomainEventArgs> args,
            CancellationToken token)
        {
            return _databaseContext
                .Insert(args.Select(BuildDatabaseDomainEvent).ToArray(), EnInsertBehavior.Default)
                .CachedExpression($"{nameof(Append)}DomainEvents:{args.Count}:EC3B8AAD-265E-46F9-A088-ABE5178FAEA5")
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

        private async Task<TAggregate?> GetAggregate<TAggregate>(
            Guid aggregateId,
            Expression<Func<DatabaseDomainEvent, bool>> versionPredicate,
            string cacheKey,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            var domainEvents = await GetEvents<TAggregate>(aggregateId, versionPredicate, cacheKey, token).ConfigureAwait(false);

            return domainEvents.Any()
                ? (TAggregate)Cctors
                    .GetOrAdd(typeof(TAggregate), static type => type.GetConstructor(new[] { typeof(IDomainEvent<TAggregate>[]) }))
                    .Invoke(new object[] { domainEvents })
                : default;
        }

        private async Task<IReadOnlyCollection<IDomainEvent<TAggregate>>> GetEvents<TAggregate>(
            Guid aggregateId,
            Expression<Func<DatabaseDomainEvent, bool>> versionPredicate,
            string cacheKey,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            var databaseDomainEvents = await _databaseContext
                .All<DatabaseDomainEvent>()
                .Where(domainEvent => domainEvent.AggregateId == aggregateId)
                .Where(versionPredicate)
                .OrderBy(domainEvent => domainEvent.Index)
                .CachedExpression(cacheKey)
                .ToListAsync(token)
                .ConfigureAwait(false);

            return databaseDomainEvents
                .Select(domainEvent => domainEvent.DomainEvent)
                .Cast<IDomainEvent<TAggregate>>()
                .ToArray();
        }
    }
}