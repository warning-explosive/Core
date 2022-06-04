namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using Core.DataAccess.Api.Transaction;
    using Core.DataAccess.Orm.Extensions;
    using DatabaseModel;
    using GenericHost.Api.Abstractions;
    using IntegrationTransport.Api.Abstractions;
    using Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;
    using EndpointIdentity = Contract.EndpointIdentity;
    using IntegrationMessage = Messaging.IntegrationMessage;

    internal class GenericEndpointOutboxHostStartupAction : IHostStartupAction
    {
        private readonly IDependencyContainer _dependencyContainer;

        public GenericEndpointOutboxHostStartupAction(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public Task Run(CancellationToken token)
        {
            var logger = _dependencyContainer.Resolve<ILogger>();
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();

            transport.BindErrorHandler(endpointIdentity, ErrorMessageHandler(_dependencyContainer, endpointIdentity, logger));

            return Task.CompletedTask;
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
            IntegrationMessage message,
            CancellationToken token)
        {
            await transaction
               .Write<InboxMessage, Guid>()
               .Update(new[] { message.ReadRequiredHeader<Id>().Value }, message => message.IsError, true, token)
               .ConfigureAwait(false);
        }

        private static Func<Exception, CancellationToken, Task> OnCatch(
            ILogger logger,
            EndpointIdentity endpointIdentity)
        {
            return (exception, _) =>
            {
                logger.Error(exception, endpointIdentity.ToString());
                return Task.CompletedTask;
            };
        }
    }
}