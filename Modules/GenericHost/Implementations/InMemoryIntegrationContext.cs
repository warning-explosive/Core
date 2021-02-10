namespace SpaceEngineers.Core.GenericHost.Implementations
{
    using System.Threading;
    using System.Threading.Tasks;
    using Core.GenericEndpoint.Abstractions;

    internal class InMemoryIntegrationContext : IIntegrationContext
    {
        private readonly InMemoryIntegrationTransport _transport;
        private readonly ManualResetEventSlim _manualResetEvent;

        public InMemoryIntegrationContext(
            InMemoryIntegrationTransport transport,
            ManualResetEventSlim manualResetEvent)
        {
            _transport = transport;
            _manualResetEvent = manualResetEvent;
        }

        public Task Send<TCommand>(TCommand integrationCommand, CancellationToken cancellationToken)
            where TCommand : IIntegrationCommand
        {
            _manualResetEvent.Wait(cancellationToken);

            return _transport.NotifyOnMessage(integrationCommand);
        }

        public Task Publish<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
            where TEvent : IIntegrationEvent
        {
            _manualResetEvent.Wait(cancellationToken);

            return _transport.NotifyOnMessage(integrationEvent);
        }
    }
}