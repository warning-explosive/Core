namespace SpaceEngineers.Core.IntegrationTransport.RabbitMQ
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using CrossCuttingConcerns.Logging;
    using Extensions;

    /// <summary>
    /// IExecutableIntegrationTransport
    /// </summary>
    internal partial class RabbitMqIntegrationTransport
    {
        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public event EventHandler<IntegrationTransportMessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Imitates start and waits for <see cref="LockTopologyConfiguration"/> method call
        /// </summary>
        /// <param name="token">CancellationToken</param>
        /// <returns>Ongoing operation</returns>
        public async Task RunBackgroundMessageProcessing(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            _cancellationRegistration = _cts.Token.Register(() =>
            {
                _logger.Information($"Cancellation was requested. {nameof(RabbitMqIntegrationTransport)} is about to dispose");

                StopBackgroundMessageProcessing(Token).Wait(Token);

                _backgroundMessageProcessingTcs.TrySetCanceled();
            });

            await _backgroundMessageProcessingTcs.Task.ConfigureAwait(false);
        }

        private async Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            _logger.Information($"{nameof(RabbitMqIntegrationTransport)} is about to start");

            try
            {
                await _sync
                    .WaitAsync(token)
                    .ConfigureAwait(false);

                await StartBackgroundMessageProcessingUnsafe(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _sync.Set();
            }
        }

        private async Task StopBackgroundMessageProcessing(CancellationToken token)
        {
            _logger.Information($"{nameof(RabbitMqIntegrationTransport)} is about to stop");

            try
            {
                await _sync
                    .WaitAsync(token)
                    .ConfigureAwait(false);

                StopBackgroundMessageProcessingUnsafe();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _sync.Set();
            }
        }

        private async Task RestartBackgroundMessageProcessing(CancellationToken token)
        {
            _logger.Information($"{nameof(RabbitMqIntegrationTransport)} is about to restart");

            try
            {
                await _sync
                    .WaitAsync(token)
                    .ConfigureAwait(false);

                StopBackgroundMessageProcessingUnsafe();

                await StartBackgroundMessageProcessingUnsafe(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _sync.Set();
            }
        }

        private async Task StartBackgroundMessageProcessingUnsafe(CancellationToken token)
        {
            Status = EnIntegrationTransportStatus.Starting;

            await _rabbitMqSettings
                .DeclareVirtualHost(_jsonSerializer, _logger, token)
                .ConfigureAwait(false);

            _connection = await ConfigureConnection(
                    _logger,
                    _rabbitMqSettings,
                    (ushort)_endpoints.Keys.Count,
                    _handleConnectionShutdownSubscription,
                    token)
                .ConfigureAwait(false);

            using (var commonChannel = ConfigureChannel(
                       _connection,
                       _rabbitMqSettings,
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { },
                       (_, _) => { }))
            {
                BuildTopology(commonChannel,
                    _rabbitMqSettings,
                    _endpoints.Keys,
                    _integrationMessageTypes);

                commonChannel.Close();
            }

            ConfigureChannels(
                _connection,
                _channels,
                _endpoints.Keys,
                _rabbitMqSettings,
                _handleChannelShutdownSubscription,
                _handleChannelCallbackExceptionSubscription,
                _handleChannelBasicReturnSubscription,
                _handleChannelBasicAcksSubscription,
                _handleChannelBasicNacksSubscription);

            StartConsumers(
                _rabbitMqSettings,
                _channels,
                _consumers,
                _handleReceivedMessageSubscription,
                _handleConsumerShutdownSubscription);

            Status = EnIntegrationTransportStatus.Running;

            _ready.Set();
        }

        private void StopBackgroundMessageProcessingUnsafe()
        {
            _ready.Reset();

            foreach (var (_, channel) in _channels)
            {
                _logger.Information($"Channel #{channel.ChannelNumber} is about to be closed");

                channel.ModelShutdown -= _handleChannelShutdownSubscription;
                channel.CallbackException -= _handleChannelCallbackExceptionSubscription;
                channel.BasicReturn -= _handleChannelBasicReturnSubscription;
                channel.BasicAcks -= _handleChannelBasicAcksSubscription;
                channel.BasicNacks -= _handleChannelBasicNacksSubscription;

                channel.Close();
                channel.Dispose();
            }

            _channels.Clear();

            var connection = Interlocked.Exchange(ref _connection, null);

            if (connection != null)
            {
                connection.ConnectionShutdown -= _handleConnectionShutdownSubscription;

                connection.Close();
                connection.Dispose();

                _logger.Information("Connection with RabbitMQ broker was successfully closed");
            }

            if (Status != EnIntegrationTransportStatus.Stopped)
            {
                Status = EnIntegrationTransportStatus.Stopped;
            }
        }
    }
}