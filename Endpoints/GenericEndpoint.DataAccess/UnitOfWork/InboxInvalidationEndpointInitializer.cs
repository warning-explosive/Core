namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot;
    using Core.DataAccess.Orm.Extensions;
    using CrossCuttingConcerns.Extensions;
    using Deduplication;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.DataAccess.Api.Transaction;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [Component(EnLifestyle.Singleton)]
    internal class InboxInvalidationEndpointInitializer : IEndpointInitializer,
                                                          ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IConfigurableIntegrationTransport _transport;
        private readonly ILogger _logger;

        public InboxInvalidationEndpointInitializer(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            IConfigurableIntegrationTransport transport,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _logger = logger;
        }

        public Task Initialize(CancellationToken token)
        {
            InitializeInboxInvalidation();

            return Task.CompletedTask;
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
            var id = integrationMessage
               .ReadRequiredHeader<Id>()
               .Value;

            await transaction
               .Write<InboxMessage>()
               .Update(message => message.IsError,
                    _ => true,
                    message => message.PrimaryKey == id,
                    token)
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