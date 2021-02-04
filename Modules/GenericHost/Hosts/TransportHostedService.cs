namespace SpaceEngineers.Core.GenericHost.Hosts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Basics;
    using GenericEndpoint.Abstractions;
    using Internals;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class TransportHostedService : IHostedService
    {
        private readonly ILogger<TransportHostedService> _logger;
        private readonly IIntegrationTransport _transport;
        private readonly IReadOnlyCollection<IGenericEndpoint> _endpoints;

        private CancellationTokenSource? _stoppingCts;

        public TransportHostedService(
            ILogger<TransportHostedService> logger,
            IIntegrationTransport transport,
            IEnumerable<IGenericEndpoint> endpoints)
        {
            _logger = logger;
            _transport = transport;
            _endpoints = endpoints.ToList();

            _transport.OnMessage += OnMessage;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var endpoint in _endpoints)
            {
                await _transport.InitializeTopology(endpoint).ConfigureAwait(false);
            }

            _logger.Information(Resources.StartedSuccessfully, _transport);
            _logger.Information(Resources.WaitingForIncomingMessages, _transport);

            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stoppingCts?.Cancel();
            _transport.OnMessage -= OnMessage;
            return Task.CompletedTask;
        }

        private void OnMessage(
            object sender,
            IntegrationMessageEventArgs args)
        {
            Dispatch(args).Wait();
        }

        private Task Dispatch(IntegrationMessageEventArgs args)
        {
            _logger.Information(Resources.DispatchMessage, args.ReflectedType, Thread.CurrentThread.ManagedThreadId);

            return this
                .CallMethod(nameof(Dispatch))
                .WithTypeArgument(args.ReflectedType)
                .WithArgument(args.Message)
                .Invoke<Task>();
        }

        private Task Dispatch<TMessage>(TMessage message)
            where TMessage : IIntegrationMessage
        {
            return _transport.Dispatch(message);
        }
    }
}