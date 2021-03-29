namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Async;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Executable;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class TransportHostedService : IHostedService, IDisposable
    {
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
            Logger = logger;
            Transport = transport;
            EndpointOptions = endpointOptions;
            Endpoints = Array.Empty<IGenericEndpoint>();

            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<IntegrationMessageEventArgs>();
        }

        private ILogger<TransportHostedService> Logger { get; }

        private IIntegrationTransport Transport { get; }

        private IReadOnlyCollection<EndpointOptions> EndpointOptions { get; }

        private IReadOnlyCollection<IGenericEndpoint> Endpoints { get; set; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _registration = _cts.Token.Register(() => _autoResetEvent.Set());

            Endpoints = await EndpointOptions
                .Select(options => Endpoint.StartAsync(InjectTransport(options, Transport), Token))
                .WhenAll()
                .ConfigureAwait(false);

            await Transport.Initialize(Endpoints, Token).ConfigureAwait(false);

            Logger.Information(Resources.StartedSuccessfully, Transport);
            Logger.Information(Resources.WaitingForIncomingMessages, Transport);

            Transport.OnMessage += OnMessage;
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
                Transport.OnMessage -= OnMessage;

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
                        .TryAsync(() => Transport.DispatchToEndpoint(args.GeneralMessage))
                        .Invoke(ex => OnError(ex, args))
                        .ConfigureAwait(false);
                }
            }
        }

        private Task OnError(Exception exception, IntegrationMessageEventArgs args)
        {
            Logger.Error(
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

        private static EndpointOptions InjectTransport(EndpointOptions options, IIntegrationTransport transport)
        {
            options.ContainerOptions ??= new DependencyContainerOptions();

            options.ContainerOptions.ManualRegistrations =
                new List<IManualRegistration>(options.ContainerOptions.ManualRegistrations)
                {
                    transport.EndpointInjection
                };

            return options;
        }
    }
}