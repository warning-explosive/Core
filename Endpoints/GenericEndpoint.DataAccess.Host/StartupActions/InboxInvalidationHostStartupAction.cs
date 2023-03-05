﻿namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using Basics.Attributes;
    using CompositionRoot;
    using Core.DataAccess.Orm.Linq;
    using Core.DataAccess.Orm.Sql.Linq;
    using Core.DataAccess.Orm.Transaction;
    using CrossCuttingConcerns.Logging;
    using Deduplication;
    using GenericEndpoint.Host.StartupActions;
    using GenericHost.Api.Abstractions;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostStartupAction))]
    internal class InboxInvalidationHostStartupAction : IHostStartupAction,
                                                        ICollectionResolvable<IHostStartupAction>,
                                                        IResolvable<InboxInvalidationHostStartupAction>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IConfigurableIntegrationTransport _transport;
        private readonly ILogger _logger;

        public InboxInvalidationHostStartupAction(
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

        public Task Run(CancellationToken token)
        {
            InitializeInboxInvalidation();

            return Task.CompletedTask;
        }

        private void InitializeInboxInvalidation()
        {
            _transport.BindErrorHandler(
                _endpointIdentity,
                ErrorMessageHandler(_dependencyContainer, _logger));
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(
            IDependencyContainer dependencyContainer,
            ILogger logger)
        {
            return (message, _, token) => HandleErrorMessage(dependencyContainer, message, token)
               .TryAsync()
               .Catch<Exception>(OnCatch(logger))
               .Invoke(token);
        }

        private static Task HandleErrorMessage(
            IDependencyContainer dependencyContainer,
            IntegrationMessage message,
            CancellationToken token)
        {
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
               .Update<InboxMessage>()
               .Set(message => message.IsError.Assign(true))
               .Where(message => message.PrimaryKey == id)
               /* TODO: .CachedExpression("5A3B2946-028C-4CA8-9E8D-1E9C3BBB1EEB")*/
               .Invoke(token)
               .ConfigureAwait(false);
        }

        private static Func<Exception, CancellationToken, Task> OnCatch(ILogger logger)
        {
            return (exception, _) =>
            {
                logger.Error(exception, "Inbox reject error");
                return Task.CompletedTask;
            };
        }
    }
}