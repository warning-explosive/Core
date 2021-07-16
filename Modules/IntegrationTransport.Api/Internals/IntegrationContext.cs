namespace SpaceEngineers.Core.IntegrationTransport.Api.Internals
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;

    [Component(EnLifestyle.Singleton)]
    internal class IntegrationContext : IIntegrationContext
    {
        private readonly IIntegrationTransport _transport;
        private readonly IIntegrationMessageFactory _factory;

        public IntegrationContext(
            IIntegrationTransport transport,
            IIntegrationMessageFactory factory)
        {
            _transport = transport;
            _factory = factory;
        }

        public Task Send<TCommand>(TCommand command, CancellationToken token)
            where TCommand : IIntegrationCommand
        {
            return Deliver(CreateGeneralMessage(command), token);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken token)
            where TEvent : IIntegrationEvent
        {
            return Deliver(CreateGeneralMessage(integrationEvent), token);
        }

        private Task Deliver(IntegrationMessage message, CancellationToken token)
        {
            return _transport.Enqueue(message, token);
        }

        private IntegrationMessage CreateGeneralMessage<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _factory.CreateGeneralMessage(message, null, null);
        }
    }
}