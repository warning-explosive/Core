﻿namespace SpaceEngineers.Core.DataAccess.Orm.ChangesTracking
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Persisting;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Scoped)]
    internal class ChangesTracker : IChangesTracker
    {
        private readonly IDependencyContainer _dependencyContainer;

        private readonly ConcurrentDictionary<Guid, IAggregate> _trackedAggregates = new ConcurrentDictionary<Guid, IAggregate>();

        public ChangesTracker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public bool HasChanges => _trackedAggregates.Any();

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
            foreach (var (_, aggregate) in _trackedAggregates)
            {
                foreach (var domainEvent in aggregate.Events)
                {
                    await this
                        .CallMethod(nameof(Persist))
                        .WithTypeArgument(domainEvent.GetType())
                        .WithArguments(domainEvent, token)
                        .Invoke<Task>()
                        .ConfigureAwait(false);
                }
            }
        }

        private Task Persist<TEvent>(TEvent domainEvent, CancellationToken token)
            where TEvent : IDomainEvent
        {
            return _dependencyContainer
                .Resolve<IDatabaseStateTransformer<TEvent>>()
                .Persist(domainEvent, token);
        }
    }
}