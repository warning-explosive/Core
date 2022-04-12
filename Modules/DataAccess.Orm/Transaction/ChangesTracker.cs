namespace SpaceEngineers.Core.DataAccess.Orm.Transaction
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class ChangesTracker : IChangesTracker,
                                    IResolvable<IChangesTracker>
    {
        private readonly IDependencyContainer _dependencyContainer;

        private readonly ConcurrentDictionary<Guid, IAggregate> _trackedAggregates = new ConcurrentDictionary<Guid, IAggregate>();

        public ChangesTracker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public bool HasChanges => _trackedAggregates.Values.SelectMany(aggregate => aggregate.Events).Any();

        public Task Track(IAggregate aggregate, CancellationToken token)
        {
            _trackedAggregates.AddOrUpdate(
                aggregate.Id,
                _ => aggregate,
                (id, existed) => throw new InvalidOperationException($"Aggregate {existed.GetType().Name} with id {id} have already been tracking in the transaction"));

            return Task.CompletedTask;
        }

        public async Task SaveChanges(CancellationToken token)
        {
            try
            {
                foreach (var (_, aggregate) in _trackedAggregates)
                {
                    foreach (var domainEvent in aggregate.Events)
                    {
                        /*
                         * TODO: #172 - persist events in event store; execute only new events (sort them by timestamps or versions)
                         */
                        await _dependencyContainer
                            .ResolveGeneric(typeof(IDomainEventHandler<>), domainEvent.GetType())
                            .CallMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))
                            .WithArguments(domainEvent, token)
                            .Invoke<Task>()
                            .ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _trackedAggregates.Clear();
        }
    }
}