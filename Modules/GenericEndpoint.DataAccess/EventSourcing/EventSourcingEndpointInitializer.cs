namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.GenericDomain.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class EventSourcingEndpointInitializer : IEndpointInitializer,
                                                      ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ITypeProvider _typeProvider;

        public EventSourcingEndpointInitializer(
            IDependencyContainer dependencyContainer,
            ITypeProvider typeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _typeProvider = typeProvider;
        }

        public Task Initialize(CancellationToken token)
        {
            InitializeDomainEventsAutoTracking(token);

            return Task.CompletedTask;
        }

        private IEnumerable<Type> Aggregates()
        {
            return _typeProvider
               .OurTypes
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IAggregate<>))
                           && type.IsConcreteType())
               .ToList();
        }

        private void InitializeDomainEventsAutoTracking(CancellationToken token)
        {
            foreach (var aggregate in Aggregates())
            {
                this
                   .CallMethod(nameof(InitializeDomainEventsAutoTracking))
                   .WithTypeArgument(aggregate)
                   .WithArgument(token)
                   .Invoke();
            }
        }

        private void InitializeDomainEventsAutoTracking<TAggregate>(CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
        {
            BaseAggregate<TAggregate>.OnDomainEvent += OnDomainEvent<TAggregate>(_dependencyContainer, token);
        }

        private static EventHandler<IDomainEvent> OnDomainEvent<TAggregate>(
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            return (_, domainEvent) => typeof(EventSourcingEndpointInitializer)
               .CallMethod(nameof(OnDomainEvent))
               .WithTypeArguments(typeof(TAggregate), domainEvent.GetType())
               .WithArguments(dependencyContainer, domainEvent, token)
               .Invoke<Task>()
               .Wait(token);
        }

        private static Task OnDomainEvent<TAggregate, TEvent>(
            IDependencyContainer dependencyContainer,
            TEvent domainEvent,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : IDomainEvent
        {
            return dependencyContainer
               .Resolve<IIntegrationContext>()
               .Send(new CaptureDomainEvent<TEvent>(domainEvent), token);
        }
    }
}