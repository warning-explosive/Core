namespace SpaceEngineers.Core.InMemoryIntegrationTransport
{
    using System.Threading.Tasks;
    using GenericEndpoint.Abstractions;

    internal class InMemoryIntegrationContext : IIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;

        public InMemoryIntegrationContext(InMemoryIntegrationTransport transport)
        {
            _transport = transport;
        }

        public Task Send<TCommand>(TCommand integrationCommand)
            where TCommand : IIntegrationCommand
        {
            return Enqueue(integrationCommand);
        }

        public Task Publish<TEvent>(TEvent integrationEvent)
            where TEvent : IIntegrationEvent
        {
            return Enqueue(integrationEvent);
        }

        private Task Enqueue<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            _transport.NotifyOnMessage(message);

            return Task.CompletedTask;
        }
    }
}