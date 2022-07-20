namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Basics;
    using Contract;
    using Messaging;
    using Messaging.Abstractions;
    using Microsoft.Extensions.Logging;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.CompositionRoot.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class GenericEndpointInitializer : IEndpointInitializer,
                                                ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTransport _transport;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly ILogger _logger;

        public GenericEndpointInitializer(
            IDependencyContainer dependencyContainer,
            EndpointIdentity endpointIdentity,
            IIntegrationTransport transport,
            IIntegrationTypeProvider integrationTypeProvider,
            ILogger logger)
        {
            _dependencyContainer = dependencyContainer;
            _endpointIdentity = endpointIdentity;
            _transport = transport;
            _integrationTypeProvider = integrationTypeProvider;
            _logger = logger;
        }

        public Task Initialize(CancellationToken token)
        {
            _transport.Bind(_endpointIdentity, MessageHandler(_dependencyContainer), _integrationTypeProvider);
            _transport.BindErrorHandler(_endpointIdentity, ErrorMessageHandler(_logger, _endpointIdentity));

            return Task.CompletedTask;
        }

        private static Func<IntegrationMessage, Task> MessageHandler(IDependencyContainer dependencyContainer)
        {
            return message => dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .ExecuteMessageHandler(message);
        }

        private static Func<IntegrationMessage, Exception, CancellationToken, Task> ErrorMessageHandler(
            ILogger logger,
            EndpointIdentity endpointIdentity)
        {
            return (_, exception, _) =>
            {
                logger.Error(exception, $"{endpointIdentity} -> Message handling error");
                return Task.CompletedTask;
            };
        }
    }
}