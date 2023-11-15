namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    /// <summary>
    /// IIntegrationTransport
    /// </summary>
    internal partial class InMemoryIntegrationTransport
    {
        public EnIntegrationTransportStatus Status
        {
            get => _status;

            private set
            {
                var previousValue = _status;

                _status = value;

                StatusChanged?.Invoke(this, new IntegrationTransportStatusChangedEventArgs(previousValue, value));
            }
        }

        public async Task<bool> Enqueue(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            if (message.ReadHeader<DeferredUntil>() != null)
            {
                await _delayedDeliveryQueue.Enqueue(message, token).ConfigureAwait(false);
            }
            else
            {
                await EnqueueInput(message, token).ConfigureAwait(false);
            }

            return true;
        }

        private Task EnqueueInput(IntegrationMessage message, CancellationToken token)
        {
            return _inputQueue.Enqueue(message, token);
        }
    }
}