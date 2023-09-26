namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.Logging;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    /// <summary>
    /// IExecutableIntegrationTransport
    /// </summary>
    internal partial class InMemoryIntegrationTransport
    {
        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public event EventHandler<IntegrationTransportMessageReceivedEventArgs>? MessageReceived;

        public Task RunBackgroundMessageProcessing(CancellationToken token)
        {
            Status = EnIntegrationTransportStatus.Starting;

            var messageProcessingTask = Task.WhenAll(
                _delayedDeliveryQueue.Run(EnqueueInput, token),
                _inputQueue.Run(HandleReceivedMessage, token));

            _ready.Set();

            Status = EnIntegrationTransportStatus.Running;

            return messageProcessingTask;
        }

        [SuppressMessage("Analysis", "CA1031", Justification = "async event handler with void as retun value")]
        private async Task HandleReceivedMessage(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            try
            {
                ManageMessageHeaders(message);

                await _topology
                    .Dispatch(message)
                    .Select(async pair =>
                    {
                        var (messageHandler, reflectedType, reason) = pair;

                        if (messageHandler == null)
                        {
                            await EnqueueError(
                                    null,
                                    message,
                                    new InvalidOperationException(reason ?? "unknown dispatch error"),
                                    token)
                                .ConfigureAwait(false);

                            return;
                        }

                        var copy = message.ContravariantClone(reflectedType);

                        await InvokeMessageHandler(messageHandler, copy)
                            .TryAsync()
                            .Catch<Exception>((exception, t) => EnqueueError(copy.ReadHeader<HandledBy>()?.Value, message, exception, t))
                            .Invoke(token)
                            .ConfigureAwait(false);
                    })
                    .WhenAll()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"{nameof(InMemoryIntegrationTransport)}.{nameof(HandleReceivedMessage)}: {message.ReadRequiredHeader<Id>().StringValue}");
            }
        }

        private static void ManageMessageHeaders(IntegrationMessage message)
        {
            message.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));
        }

        private async Task InvokeMessageHandler(
            Func<IntegrationMessage, Task> messageHandler,
            IntegrationMessage message)
        {
            OnMessageReceived(message, default);

            try
            {
                await messageHandler
                    .Invoke(message)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            var rejectReason = message.ReadHeader<RejectReason>();

            if (rejectReason?.Value != null)
            {
                throw rejectReason.Value.Rethrow();
            }
        }

        private async Task EnqueueError(
            EndpointIdentity? endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            OnMessageReceived(message, exception);

            if (endpointIdentity == null)
            {
                _logger.Error(exception, $"{nameof(InMemoryIntegrationTransport)}.{nameof(EnqueueError)}: {message.ReadRequiredHeader<Id>().StringValue}");
                return;
            }

            var (isSuccess, reason) = await _topology
                .TryHandleError(endpointIdentity, message, exception, token)
                .ConfigureAwait(false);

            if (isSuccess)
            {
                return;
            }

            _logger.Error(new InvalidOperationException(reason ?? "unknown error handling error"), $"{nameof(InMemoryIntegrationTransport)}.{nameof(EnqueueError)}: {message.ReadRequiredHeader<Id>().StringValue}");
        }

        private void OnMessageReceived(IntegrationMessage integrationMessage, Exception? exception)
        {
            MessageReceived?.Invoke(this, new IntegrationTransportMessageReceivedEventArgs(integrationMessage, exception));
        }
    }
}