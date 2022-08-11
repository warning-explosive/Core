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

        private IReadOnlyDictionary<Type, EventHandler<IDomainEvent>> _subscriptions;

        public EventSourcingEndpointInitializer(
            IDependencyContainer dependencyContainer,
            ITypeProvider typeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _typeProvider = typeProvider;

            _subscriptions = new Dictionary<Type, EventHandler<IDomainEvent>>();
        }

        public Task Initialize(CancellationToken token)
        {
            _subscriptions = _typeProvider
               .OurTypes
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IAggregate<>))
                           && type.IsConcreteType())
               .ToDictionary(type => type, type => OnDomainEvent(_dependencyContainer, type, token));

            foreach (var (aggregate, subscription) in _subscriptions)
            {
                Subscribe(aggregate, subscription);

                token.Register(() => Unsubscribe(aggregate, subscription));
            }

            return Task.CompletedTask;
        }

        private static void Subscribe(Type aggregate, EventHandler<IDomainEvent> subscription)
        {
            typeof(EventSourcingEndpointInitializer)
               .CallMethod(nameof(Subscribe))
               .WithTypeArgument(aggregate)
               .WithArguments(subscription)
               .Invoke();
        }

        private static void Unsubscribe(Type aggregate, EventHandler<IDomainEvent> subscription)
        {
            typeof(EventSourcingEndpointInitializer)
               .CallMethod(nameof(Unsubscribe))
               .WithTypeArgument(aggregate)
               .WithArguments(subscription)
               .Invoke();
        }

        private static void Subscribe<TAggregate>(EventHandler<IDomainEvent> subscription)
            where TAggregate : class, IAggregate<TAggregate>
        {
            BaseAggregate<TAggregate>.OnDomainEvent += subscription;
        }

        private static void Unsubscribe<TAggregate>(EventHandler<IDomainEvent> subscription)
            where TAggregate : class, IAggregate<TAggregate>
        {
            BaseAggregate<TAggregate>.OnDomainEvent -= subscription;
        }

        private static EventHandler<IDomainEvent> OnDomainEvent(
            IDependencyContainer dependencyContainer,
            Type aggregate,
            CancellationToken token)
        {
            return (_, domainEvent) =>
            {
                typeof(EventSourcingEndpointInitializer)
                   .CallMethod(nameof(OnDomainEvent))
                   .WithTypeArguments(aggregate, domainEvent.GetType())
                   .WithArguments(dependencyContainer, domainEvent, token)
                   .Invoke<Task>()
                   .Wait(token);
            };
        }

        private static Task OnDomainEvent<TAggregate, TEvent>(
            IDependencyContainer dependencyContainer,
            TEvent domainEvent,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent
        {
            return dependencyContainer
               .Resolve<IIntegrationContext>()
               .Send(new CaptureDomainEvent<TEvent>(domainEvent), token);
        }
    }
}