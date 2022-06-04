namespace SpaceEngineers.Core.GenericHost.Test.Mocks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
    using SpaceEngineers.Core.IntegrationTransport.Api.Abstractions;
    using SpaceEngineers.Core.IntegrationTransport.Api.Enumerations;

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

        public void Bind(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, CancellationToken, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            Decoratee.Bind(endpointIdentity, messageHandler, integrationTypeProvider);
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            Decoratee.BindErrorHandler(endpointIdentity, errorMessageHandler);
        }

        public async Task<bool> Enqueue(IntegrationMessage message, CancellationToken token)
        {
            var wasSent = await Decoratee
               .Enqueue(message, token)
               .ConfigureAwait(false);

            if (wasSent)
            {
                await _collector
                   .Collect(message, null, token)
                   .ConfigureAwait(false);
            }

            return wasSent;
        }

        public async Task EnqueueError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            await Decoratee
               .EnqueueError(endpointIdentity, message, exception, token)
               .ConfigureAwait(false);

            await _collector
               .Collect(message, exception, token)
               .ConfigureAwait(false);
        }

        public Task Accept(IntegrationMessage message, CancellationToken token)
        {
            return Decoratee.Accept(message, token);
        }

        public Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            return Decoratee.StartBackgroundMessageProcessing(token);
        }
    }
}