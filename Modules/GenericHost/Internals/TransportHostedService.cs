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
        private readonly IReadOnlyCollection<IGenericEndpoint> _endpoints;

        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<IntegrationMessageEventArgs> _queue;
        private Task? _messageProcessingTask;
        private CancellationTokenSource? _cts;
        private IDisposable? _registration;

        public TransportHostedService(
            ILogger<TransportHostedService> logger,
            IIntegrationTransport transport,
            IEnumerable<IGenericEndpoint> endpoints)
        {
            _logger = logger;
            _transport = transport;
            _endpoints = endpoints.ToList();

            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<IntegrationMessageEventArgs>();
        }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _registration = _cts.Token.Register(() => _autoResetEvent.Set());

            await _transport.Initialize(_endpoints, Token).ConfigureAwait(false);

            _logger.Information(Resources.StartedSuccessfully, _transport);
            _logger.Information(Resources.WaitingForIncomingMessages, _transport);

            _transport.OnMessage += OnMessage;

            _messageProcessingTask = StartMessageProcessing();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
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
                    .WhenAny(_messageProcessingTask, Task.Delay(Timeout.Infinite, cancellationToken))
                    .ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _messageProcessingTask?.Dispose();
            _registration?.Dispose();
            _cts?.Dispose();
        }

        private Task StartMessageProcessing()
        {
            while (!Token.IsCancellationRequested)
            {
                _autoResetEvent.WaitAsync();

                if (_queue.TryDequeue(out var args))
                {
                    DispatchToEndpointUnsafe(args)
                        .Try()
                        .Invoke(ex => _logger.Error(ex, "Transport error on message: {0} {1}", args.ReflectedType, args.Message));
                }
            }

            return Task.CompletedTask;
        }

        private Action DispatchToEndpointUnsafe(IntegrationMessageEventArgs args)
        {
            return () => _transport
                .CallMethod(nameof(IIntegrationTransport.DispatchToEndpoint))
                .WithTypeArgument(args.ReflectedType)
                .WithArgument(args.Message)
                .Invoke<Task>()
                .Wait(Token); // unwrap error
        }

        private void OnMessage(object? sender, IntegrationMessageEventArgs args)
        {
            _queue.Enqueue(args);
            _autoResetEvent.Set();
        }
    }
}