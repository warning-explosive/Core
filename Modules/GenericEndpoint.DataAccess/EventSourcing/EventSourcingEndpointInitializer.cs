namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Api.Exceptions;
    using Contract;
    using Microsoft.Extensions.Logging;
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
        private readonly EndpointIdentity _endpointIdentity;
        private readonly ITypeProvider _typeProvider;
        private readonly ILogger _logger;

        public EventSourcingEndpointInitializer(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            ITypeProvider typeProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _typeProvider = typeProvider;
            _logger = logger;
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
            BaseAggregate<TAggregate>.OnDomainEvent += OnDomainEvent<TAggregate>(_dependencyContainer, _endpointIdentity, _logger, token);
        }

        private static EventHandler<IDomainEvent> OnDomainEvent<TAggregate>(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            ILogger logger,
            CancellationToken token)
        {
            return (_, domainEvent) => typeof(EventSourcingEndpointInitializer)
               .CallMethod(nameof(OnDomainEvent))
               .WithTypeArguments(typeof(TAggregate), domainEvent.GetType())
               .WithArguments(dependencyContainer, endpointIdentity, logger, domainEvent, token)
               .Invoke<Task>()
               .Wait(token);
        }

        private static async Task OnDomainEvent<TAggregate, TEvent>(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            ILogger logger,
            TEvent domainEvent,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent
        {
            try
            {
                await dependencyContainer
                   .Resolve<IIntegrationContext>()
                   .Send(new CaptureDomainEvent<TEvent>(domainEvent), token)
                   .ConfigureAwait(false);
            }
            catch (ComponentResolutionException exception)
            {
                logger.Error(exception, $"{endpointIdentity} -> Don't populate domain events outside of message handler scope");
            }
        }
    }
}