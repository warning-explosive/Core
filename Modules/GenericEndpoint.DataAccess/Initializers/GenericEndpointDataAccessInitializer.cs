namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Initializers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using Basics;
    using DatabaseModel;
    using EventSourcing;
    using GenericDomain.Api.Abstractions;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.DataAccess.Api.Transaction;
    using SpaceEngineers.Core.DataAccess.Orm.Extensions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointDataAccessInitializer : IEndpointInitializer,
                                                          ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly ITypeProvider _typeProvider;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTransport _transport;
        private readonly ILogger _logger;

        public GenericEndpointDataAccessInitializer(
            IDependencyContainer dependencyContainer,
            ITypeProvider typeProvider,
            EndpointIdentity endpointIdentity,
            IIntegrationTransport transport,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _typeProvider = typeProvider;
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _logger = logger;
        }

        public Task Initialize(CancellationToken token)
        {
            InitializeAggregatesAutoTracking(token);
            InitializeInboxInvalidation();

            return Task.CompletedTask;
        }

        private void InitializeAggregatesAutoTracking(CancellationToken token)
        {
            var aggregates = _typeProvider
               .OurTypes
               .Where(type => type.IsSubclassOfOpenGeneric(typeof(IAggregate<>))
                           && type.IsConcreteType())
               .ToList();

            foreach (var aggregate in aggregates)
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
            BaseAggregate<TAggregate>.OnDomainEvent += OnDomainEvent<TAggregate>(token);
        }

        private EventHandler<IDomainEvent> OnDomainEvent<TAggregate>(CancellationToken token)
        {
            return (_, domainEvent) => typeof(GenericEndpointDataAccessInitializer)
               .CallMethod(nameof(OnDomainEvent))
               .WithTypeArguments(typeof(TAggregate), domainEvent.GetType())
               .WithArguments(_dependencyContainer, domainEvent, token)
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

        private void InitializeInboxInvalidation()
        {
            _transport.BindErrorHandler(
                _endpointIdentity,
                ErrorMessageHandler(_dependencyContainer, _endpointIdentity, _logger));
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            ILogger logger)
        {
            return (message, _, token) => ExecutionExtensions
               .TryAsync((dependencyContainer, message), HandleErrorMessage)
               .Catch<Exception>(OnCatch(logger, endpointIdentity))
               .Invoke(token);
        }

        private static Task HandleErrorMessage(
            (IDependencyContainer, IntegrationMessage) state,
            CancellationToken token)
        {
            var (dependencyContainer, message) = state;

            return dependencyContainer.InvokeWithinTransaction(true,
                message,
                HandleErrorMessage,
                token);
        }

        private static async Task HandleErrorMessage(
            IDatabaseTransaction transaction,
            IntegrationMessage integrationMessage,
            CancellationToken token)
        {
            await transaction
               .Write<InboxMessage, Guid>()
               .Update(new[] { integrationMessage.ReadRequiredHeader<Id>().Value }, message => message.IsError, true, token)
               .ConfigureAwait(false);
        }

        private static Func<Exception, CancellationToken, Task> OnCatch(
            ILogger logger,
            EndpointIdentity endpointIdentity)
        {
            return (exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Inbox reject error");
                return Task.CompletedTask;
            };
        }
    }
}