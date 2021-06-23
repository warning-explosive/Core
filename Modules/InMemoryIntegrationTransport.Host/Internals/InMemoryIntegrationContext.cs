namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Host.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericHost.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationContext : IIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Deliver(command, token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Deliver(integrationEvent, token);
        }

        private Task Deliver<TMessage>(TMessage message, CancellationToken token)
            where TMessage : IIntegrationMessage
        {
            var integrationMessage = _factory.CreateGeneralMessage(message, null, null);

            return _transport.Enqueue(integrationMessage, token);
        }
    }
}