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
            var endpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();
            var integrationTypeProvider = _dependencyContainer.Resolve<IIntegrationTypeProvider>();

            transport.Bind(endpointIdentity, ExecuteRpcReplyMessageHandlers(_dependencyContainer), integrationTypeProvider);
            transport.BindErrorHandler(endpointIdentity, ErrorMessageHandler(logger, endpointIdentity));

            transport.StatusChanged += OnStatusChanged(logger);

            await _dependencyContainer
                .Resolve<IIntegrationTransport>()
                .StartBackgroundMessageProcessing(token)
                .ConfigureAwait(false);
        }

        private static EventHandler<IntegrationTransportStatusChangedEventArgs> OnStatusChanged(ILogger logger)
        {
            return (sender, args) => logger.Information($"{sender.GetType().Name}: {args.PreviousStatus} -> {args.CurrentStatus}");
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
            EndpointIdentity endpointIdentity)
        {
            return (_, exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Message dispatching error");
                return Task.CompletedTask;
            };
        }
    }
}