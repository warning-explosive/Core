namespace SpaceEngineers.Core.GenericHost.Implementations
{
    using System.Threading;
    using System.Threading.Tasks;
    using Basics.Async;
    using Core.GenericEndpoint.Abstractions;

    internal class InMemoryIntegrationContext : IIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly AsyncManualResetEvent _manualResetEvent;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            AsyncManualResetEvent manualResetEvent)
        {
            _transport = transport;
            _manualResetEvent = manualResetEvent;
        }

        public Task Send<TCommand>(TCommand integrationCommand, CancellationToken cancellationToken)
            where TCommand : IIntegrationCommand
        {
            return Notify(integrationCommand, cancellationToken);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
            where TEvent : IIntegrationEvent
        {
            return Notify(integrationEvent, cancellationToken);
        }

        private async Task Notify<TMessage>(TMessage integrationMessage, CancellationToken cancellationToken)
            where TMessage : IIntegrationMessage
        {
            await _manualResetEvent.WaitAsync(cancellationToken).ConfigureAwait(false);

            _transport.NotifyOnMessage(integrationMessage);
        }
    }
}