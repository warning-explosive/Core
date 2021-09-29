namespace SpaceEngineers.Core.Modules.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Api.Enumerations;

    [ManuallyRegisteredComponent(nameof(GenericHost))]
    internal class IntegrationTransportMessagesCollectorDecorator : IIntegrationTransport,
                                                                    IDecorator<IIntegrationTransport>
    {
        private readonly MessagesCollector _collector;

        public IntegrationTransportMessagesCollectorDecorator(
            IIntegrationTransport decoratee,
            MessagesCollector collector)
        {
            Decoratee = decoratee;
            Decoratee.StatusChanged += (s, e) => StatusChanged?.Invoke(s, e);

            _collector = collector;
        }

        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public IIntegrationTransport Decoratee { get; }

        public EnIntegrationTransportStatus Status => Decoratee.Status;

        public void Bind(Type message, EndpointIdentity endpointIdentity, Func<IntegrationMessage, Task> messageHandler)
        {
            Decoratee.Bind(message, endpointIdentity, messageHandler);
        }

        public void BindErrorHandler(EndpointIdentity endpointIdentity, Func<IntegrationMessage, Task> errorMessageHandler)
        {
            Decoratee.BindErrorHandler(endpointIdentity, errorMessageHandler);
        }

        public async Task Enqueue(IntegrationMessage message, CancellationToken token)
        {
            await Decoratee.Enqueue(message, token).ConfigureAwait(false);
            await _collector.Collect(message, null, token).ConfigureAwait(false);
        }

        public async Task EnqueueError(IntegrationMessage message, Exception exception, CancellationToken token)
        {
            await Decoratee.EnqueueError(message, exception, token).ConfigureAwait(false);
            await _collector.Collect(message, exception, token).ConfigureAwait(false);
        }

        public Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            return Decoratee.StartBackgroundMessageProcessing(token);
        }
    }
}