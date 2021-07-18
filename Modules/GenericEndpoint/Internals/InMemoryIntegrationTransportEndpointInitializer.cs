namespace SpaceEngineers.Core.GenericEndpoint.Internals
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Api.Abstractions;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Contract;
    using IntegrationTransport.Api.Abstractions;
    using Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransportEndpointInitializer : IEndpointInitializer,
                                                                     ICollectionResolvable<IEndpointInitializer>
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly IIntegrationTransport _transport;
        private readonly IDependencyContainer _dependencyContainer;

        public InMemoryIntegrationTransportEndpointInitializer(
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
            // TODO: handle replies
            _integrationTypeProvider.EndpointCommands()
                .Concat(_integrationTypeProvider.EndpointQueries())
                .Concat(_integrationTypeProvider.EndpointSubscriptions())
                .Each(message => _transport.Bind(message, _endpointIdentity, ProcessMessage));

            return Task.CompletedTask;
        }

        private Task ProcessMessage(IntegrationMessage message)
        {
            return _dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .ProcessMessage(message);
        }
    }
}