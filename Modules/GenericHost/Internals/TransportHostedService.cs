namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics;
    using Basics.Async;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class TransportHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<TransportHostedService> _logger;
        private readonly IIntegrationTransport _transport;
        private readonly IReadOnlyCollection<EndpointOptions> _endpointOptions;

        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<IntegrationMessageEventArgs> _queue;
        private Task? _messageProcessingTask;
        private CancellationTokenSource? _cts;
        private IDisposable? _registration;

        public TransportHostedService(
            ILogger<TransportHostedService> logger,
            IIntegrationTransport transport,
            IReadOnlyCollection<EndpointOptions> endpointOptions)
        {
            _logger = logger;
            _transport = transport;
            _endpointOptions = endpointOptions;

            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<IntegrationMessageEventArgs>();
        }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _registration = _cts.Token.Register(() => _autoResetEvent.Set());

            var endpoints = await Task
                .WhenAll(_endpointOptions.Select(options => Endpoint.StartAsync(options, Token)))
                .ConfigureAwait(false);

            await _transport.Initialize(endpoints, Token).ConfigureAwait(false);

            _logger.Information(Resources.StartedSuccessfully, _transport);
            _logger.Information(Resources.WaitingForIncomingMessages, _transport);

            _transport.OnMessage += OnMessage;
            _messageProcessingTask = StartMessageProcessing();
        }

        public async Task StopAsync(CancellationToken token)
        {
            if (_messageProcessingTask == null)
            {
                // Stop called without start
                return;
            }

            try
            {
                // Unsubscribe from transport event
                _transport.OnMessage -= OnMessage;

                // Signal cancellation to the executing method
                _cts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task
                    .WhenAny(_messageProcessingTask, Task.Delay(Timeout.Infinite, token))
                    .ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _messageProcessingTask?.Dispose();
            _registration?.Dispose();
            _cts?.Dispose();
        }

        private async Task StartMessageProcessing()
        {
            while (!Token.IsCancellationRequested)
            {
                await _autoResetEvent.WaitAsync().ConfigureAwait(false);

                // TODO: use async queue
                if (_queue.TryDequeue(out var args))
                {
                    await ExecutionExtensions
                        .TryAsync(() => _transport.DispatchToEndpoint(args.GeneralMessage))
                        .Invoke(ex => OnError(ex, args))
                        .ConfigureAwait(false);
                }
            }
        }

        private Task OnError(Exception exception, IntegrationMessageEventArgs args)
        {
            _logger.Error(
                exception,
                "Transport error on message: {0} {1}",
                args.GeneralMessage.ReflectedType,
                args.GeneralMessage.Payload);

            return Task.CompletedTask;
        }

        private void OnMessage(object? sender, IntegrationMessageEventArgs args)
        {
            _queue.Enqueue(args);
            _autoResetEvent.Set();
        }
    }
}