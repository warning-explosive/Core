namespace SpaceEngineers.Core.GenericEndpoint.Endpoint
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Contract;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

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
            _integrationTypeProvider.EndpointCommands()
                .Concat(_integrationTypeProvider.EndpointQueries())
                .Concat(_integrationTypeProvider.RepliesSubscriptions())
                .Concat(_integrationTypeProvider.EventsSubscriptions())
                .Each(message => _transport.Bind(message, _endpointIdentity, ExecuteMessageHandlers));

            return Task.CompletedTask;
        }

        private Task ExecuteMessageHandlers(IntegrationMessage message, CancellationToken token)
        {
            return _dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .ExecuteMessageHandlers(message, token);
        }
    }
}