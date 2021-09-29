namespace SpaceEngineers.Core.IntegrationTransport.Host.BackgroundWorkers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericHost.Api.Abstractions;
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
            await BindRpcReplyMessageHandlers(token)
                .ConfigureAwait(false);

            await _dependencyContainer
                .Resolve<IIntegrationTransport>()
                .StartBackgroundMessageProcessing(token)
                .ConfigureAwait(false);
        }

        private Task BindRpcReplyMessageHandlers(CancellationToken token)
        {
            var replies = AssembliesExtensions
                .AllOurAssembliesFromCurrentDomain()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IIntegrationReply).IsAssignableFrom(type)
                               && !IsMessageContractAbstraction(type))
                .ToList();

            var transport = _dependencyContainer.Resolve<IIntegrationTransport>();
            var transportEndpointIdentity = _dependencyContainer.Resolve<EndpointIdentity>();

            foreach (Type reply in replies)
            {
                this
                    .CallMethod(nameof(Bind))
                    .WithTypeArgument(reply)
                    .WithArguments(transport, transportEndpointIdentity, token)
                    .Invoke();
            }

            return Task.CompletedTask;
        }

        private static bool IsMessageContractAbstraction(Type type)
        {
            return type == typeof(IIntegrationMessage)
                   || type == typeof(IIntegrationCommand)
                   || type == typeof(IIntegrationEvent)
                   || type == typeof(IIntegrationReply)
                   || typeof(IIntegrationQuery<>) == type.GenericTypeDefinitionOrSelf();
        }

        private void Bind<TReply>(
            IIntegrationTransport transport,
            EndpointIdentity transportEndpointIdentity,
            CancellationToken token)
            where TReply : IIntegrationReply
        {
            transport.Bind(typeof(TReply), transportEndpointIdentity, ExecuteMessageHandlers<TReply>(token));
        }

        private Func<IntegrationMessage, Task> ExecuteMessageHandlers<TReply>(CancellationToken token)
            where TReply : IIntegrationReply
        {
            return message => _dependencyContainer
                .Resolve<IRpcReplyMessageHandler<TReply>>()
                .Handle(message, token);
        }
    }
}