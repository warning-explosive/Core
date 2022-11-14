namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using EventSourcing;
    using GenericEndpoint.Host.StartupActions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.GenericDomain.Api.Abstractions;
    using SpaceEngineers.Core.GenericHost.Api.Abstractions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostStartupAction))]
    internal class EventSourcingHostStartupAction : IHostStartupAction,
                                                    ICollectionResolvable<IHostStartupAction>,
                                                    IResolvable<EventSourcingHostStartupAction>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ITypeProvider _typeProvider;

        public EventSourcingHostStartupAction(
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
                var subscription = OnDomainEvent(_dependencyContainer, aggregate, token);

                Subscribe(aggregate, subscription);

                token.Register(() => Unsubscribe(aggregate, subscription));
            }

            return Task.CompletedTask;
        }

        private static void Subscribe(Type aggregate, EventHandler<DomainEventArgs> subscription)
        {
            typeof(EventSourcingHostStartupAction)
               .CallMethod(nameof(Subscribe))
               .WithTypeArgument(aggregate)
               .WithArguments(subscription)
               .Invoke();
        }

        private static void Unsubscribe(Type aggregate, EventHandler<DomainEventArgs> subscription)
        {
            typeof(EventSourcingHostStartupAction)
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
            Type aggregate,
            CancellationToken token)
        {
            return (_, args) =>
            {
                typeof(EventSourcingHostStartupAction)
                   .CallMethod(nameof(OnDomainEvent))
                   .WithTypeArguments(aggregate, args.DomainEvent.GetType())
                   .WithArguments(dependencyContainer, args, token)
                   .Invoke<Task>()
                   .Wait(token);
            };
        }

        private static Task OnDomainEvent<TAggregate, TEvent>(
            IDependencyContainer dependencyContainer,
            DomainEventArgs args,
            CancellationToken token)
            where TAggregate : class, IAggregate<TAggregate>
            where TEvent : class, IDomainEvent<TAggregate>
        {
            return dependencyContainer
               .Resolve<IIntegrationContext>()
               .Send(new CaptureDomainEvent<TAggregate, TEvent>(args), token);
        }
    }
}