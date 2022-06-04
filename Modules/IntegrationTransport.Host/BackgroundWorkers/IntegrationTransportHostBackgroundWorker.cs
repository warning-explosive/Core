namespace SpaceEngineers.Core.IntegrationTransport.Host.BackgroundWorkers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericHost.Api.Abstractions;
    using Microsoft.Extensions.Logging;
    using RpcRequest;

    internal class IntegrationTransportHostBackgroundWorker : IHostBackgroundWorker
    {
        private readonly IDependencyContainer _dependencyContainer;

        public IntegrationTransportHostBackgroundWorker(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public async Task Run(CancellationToken token)
        {
            var logger = _dependencyContainer.Resolve<ILogger>();
            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var transportEndpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();
            var integrationTypeProvider = _dependencyContainer.Resolve<IIntegrationTypeProvider>();

            transport.Bind(transportEndpointIdentity, ExecuteRpcReplyMessageHandlers(_dependencyContainer), integrationTypeProvider);
            transport.BindErrorHandler(transportEndpointIdentity, ErrorMessageHandler(logger, transportEndpointIdentity));

            await _dependencyContainer
                .Resolve<IIntegrationTransport>()
                .StartBackgroundMessageProcessing(token)
                .ConfigureAwait(false);
        }

        private static Func<IntegrationMessage, CancellationToken, Task> ExecuteRpcReplyMessageHandlers(
            IDependencyContainer dependencyContainer)
        {
            return (message, token) => typeof(IntegrationTransportHostBackgroundWorker)
               .CallMethod(nameof(ExecuteRpcReplyMessageHandlers))
               .WithTypeArgument(message.ReflectedType)
               .WithArguments(dependencyContainer, message, token)
               .Invoke<Task>();
        }

        private static async Task ExecuteRpcReplyMessageHandlers<TReply>(
            IDependencyContainer dependencyContainer,
            IntegrationMessage message,
            CancellationToken token)
            where TReply : IIntegrationReply
        {
            await using (dependencyContainer.OpenScopeAsync().ConfigureAwait(false))
            {
                await dependencyContainer
                   .Resolve<IRpcReplyMessageHandler<TReply>>()
                   .Handle(message, token)
                   .ConfigureAwait(false);
            }
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(
            ILogger logger,
            EndpointIdentity transportEndpointIdentity)
        {
            return (_, exception, _) =>
            {
                logger.Error(exception, transportEndpointIdentity.ToString());
                return Task.CompletedTask;
            };
        }
    }
}