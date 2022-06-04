namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions;
    using Contract;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;
    using Messaging.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationTransportEndpointInitializer : IEndpointInitializer,
                                                             ICollectionResolvable<IEndpointInitializer>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IIntegrationTransport _transport;
        private readonly IDependencyContainer _dependencyContainer;

        public IntegrationTransportEndpointInitializer(
            IIntegrationTransport transport,
            EndpointIdentity endpointIdentity,
            IIntegrationTypeProvider integrationTypeProvider,
            IDependencyContainer dependencyContainer)
        {
            _transport = transport;
            _endpointIdentity = endpointIdentity;
            _integrationTypeProvider = integrationTypeProvider;
            _dependencyContainer = dependencyContainer;
        }

        public Task Initialize(CancellationToken token)
        {
            _transport.Bind(_endpointIdentity, ExecuteMessageHandlers(_dependencyContainer), _integrationTypeProvider);

            return Task.CompletedTask;
        }

        private static Func<IntegrationMessage, CancellationToken, Task> ExecuteMessageHandlers(
            IDependencyContainer dependencyContainer)
        {
            return (message, token) => dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .ExecuteMessageHandlers(message, token);
        }
    }
}