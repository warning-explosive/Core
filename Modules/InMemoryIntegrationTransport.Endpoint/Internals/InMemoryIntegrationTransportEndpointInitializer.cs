namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Endpoint.Internals
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Api.Abstractions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransportEndpointInitializer : IEndpointInitializer
    {
        private readonly EndpointIdentity _endpointIdentity;
        private readonly IIntegrationTypeProvider _integrationTypeProvider;
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IDependencyContainer _dependencyContainer;

        public InMemoryIntegrationTransportEndpointInitializer(
            EndpointIdentity endpointIdentity,
            IIntegrationTypeProvider integrationTypeProvider,
            InMemoryIntegrationTransport transport,
            IDependencyContainer dependencyContainer)
        {
            _endpointIdentity = endpointIdentity;
            _integrationTypeProvider = integrationTypeProvider;
            _transport = transport;
            _dependencyContainer = dependencyContainer;
        }

        public Task Initialize(CancellationToken token)
        {
            _integrationTypeProvider.EndpointCommands()
                .Concat(_integrationTypeProvider.EndpointQueries())
                .Concat(_integrationTypeProvider.EndpointSubscriptions())
                .Each(message => _transport.Bind(message, _endpointIdentity, InvokeMessageHandler));

            return Task.CompletedTask;
        }

        private Task InvokeMessageHandler(IntegrationMessage message)
        {
            return _dependencyContainer
                .Resolve<IExecutableEndpoint>()
                .InvokeMessageHandler(message);
        }
    }
}