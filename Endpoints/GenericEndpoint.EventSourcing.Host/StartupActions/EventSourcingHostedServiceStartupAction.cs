namespace SpaceEngineers.Core.GenericEndpoint.EventSourcing.Host.StartupActions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using GenericDomain.Api.Abstractions;
    using GenericDomain.EventSourcing;
    using GenericEndpoint.Host.StartupActions;
    using GenericHost;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostedServiceStartupAction))]
    internal class EventSourcingHostedServiceStartupAction : IHostedServiceStartupAction,
                                                             ICollectionResolvable<IHostedServiceObject>,
                                                             ICollectionResolvable<IHostedServiceStartupAction>,
                                                             IResolvable<EventSourcingHostedServiceStartupAction>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ITypeProvider _typeProvider;

        public EventSourcingHostedServiceStartupAction(
            IDependencyContainer dependencyContainer,
            ITypeProvider typeProvider)
        {
            _dependencyContainer = dependencyContainer;
            _typeProvider = typeProvider;
        }

        public Task Run(CancellationToken token)
        {
            var aggregates = _typeProvider
               .OurTypes
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IAggregate<>))
                           && type.IsConcreteType());

            foreach (var aggregate in aggregates)
            {
                var subscription = OnDomainEvent(_dependencyContainer, token);

                Subscribe(aggregate, subscription);

                token.Register(() => Unsubscribe(aggregate, subscription));
            }

            return Task.CompletedTask;
        }

        private static void Subscribe(Type aggregate, EventHandler<DomainEventArgs> subscription)
        {
            typeof(EventSourcingHostedServiceStartupAction)
               .CallMethod(nameof(Subscribe))
               .WithTypeArgument(aggregate)
               .WithArguments(subscription)
               .Invoke();
        }

        private static void Unsubscribe(Type aggregate, EventHandler<DomainEventArgs> subscription)
        {
            typeof(EventSourcingHostedServiceStartupAction)
               .CallMethod(nameof(Unsubscribe))
               .WithTypeArgument(aggregate)
               .WithArguments(subscription)
               .Invoke();
        }

        private static void Subscribe<TAggregate>(EventHandler<DomainEventArgs> subscription)
            where TAggregate : class, IAggregate<TAggregate>
        {
            BaseAggregate<TAggregate>.OnDomainEvent += subscription;
        }

        private static void Unsubscribe<TAggregate>(EventHandler<DomainEventArgs> subscription)
            where TAggregate : class, IAggregate<TAggregate>
        {
            BaseAggregate<TAggregate>.OnDomainEvent -= subscription;
        }

        private static EventHandler<DomainEventArgs> OnDomainEvent(
            IDependencyContainer dependencyContainer,
            CancellationToken token)
        {
            return (_, args) =>
            {
                // TODO: #217 - make async callback / append to collection and insert on commit
                dependencyContainer
                    .Resolve<IEventStore>()
                    .Append(args, token)
                    .Wait(token);
            };
        }
    }
}