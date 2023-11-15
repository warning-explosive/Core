namespace SpaceEngineers.Core.GenericEndpoint.Host.StartupActions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Attributes;
    using CompositionRoot;
    using Contract;
    using CrossCuttingConcerns.Logging;
    using Endpoint;
    using GenericHost;
    using Messaging;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

    [ManuallyRegisteredComponent("Hosting dependency that implicitly participates in composition")]
    [Before(typeof(GenericEndpointHostedServiceStartupAction))]
    internal class MessagingHostedServiceStartupAction : IHostedServiceStartupAction,
                                                         ICollectionResolvable<IHostedServiceObject>,
                                                         ICollectionResolvable<IHostedServiceStartupAction>,
                                                         IResolvable<MessagingHostedServiceStartupAction>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IConfigurableIntegrationTransport _transport;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly ILogger _logger;

        public MessagingHostedServiceStartupAction(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            IConfigurableIntegrationTransport transport,
            IIntegrationTypeProvider integrationTypeProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _integrationTypeProvider = integrationTypeProvider;
            _logger = logger;
        }

        public Task Run(CancellationToken token)
        {
            _transport.BindMessageHandler(_endpointIdentity, MessageHandler(_dependencyContainer), _integrationTypeProvider);
            _transport.BindErrorHandler(_endpointIdentity, ErrorMessageHandler(_logger));
            _transport.LockTopologyConfiguration(_endpointIdentity);

            return Task.CompletedTask;
        }

        private static Func<IntegrationMessage, Task> MessageHandler(IDependencyContainer dependencyContainer)
        {
            return message => dependencyContainer
                .Resolve<IGenericEndpoint>()
                .ExecuteMessageHandler(message);
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(ILogger logger)
        {
            return (message, exception, _) =>
            {
                logger.Error(exception, $"Message handling error: {message.ReflectedType.FullName}");
                return Task.CompletedTask;
            };
        }
    }
}