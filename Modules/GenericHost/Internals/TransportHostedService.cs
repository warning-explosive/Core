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
    using Basics.Exceptions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Executable;
    using GenericEndpoint.Executable.Abstractions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class TransportHostedService : IHostedService, IDisposable
    {
        private readonly AsyncAutoResetEvent _autoResetEvent;
        private readonly ConcurrentQueue<IntegrationMessageEventArgs> _queue;

        private IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> _topologyMap;
        private Task? _messageProcessingTask;
        private CancellationTokenSource? _cts;
        private IDisposable? _registration;

        public TransportHostedService(
            ILogger<TransportHostedService> logger,
            IAdvancedIntegrationTransport transport,
            IEndpointInstanceSelectionBehavior selectionBehavior,
            IHostStatistics statistics,
            IReadOnlyCollection<EndpointOptions> endpointOptions)
        {
            Logger = logger;
            Transport = transport;
            SelectionBehavior = selectionBehavior;
            Statistics = statistics;
            EndpointOptions = endpointOptions;
            Endpoints = Array.Empty<IGenericEndpoint>();

            _topologyMap = new Dictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>();
            _autoResetEvent = new AsyncAutoResetEvent(false);
            _queue = new ConcurrentQueue<IntegrationMessageEventArgs>();
        }

        private ILogger<TransportHostedService> Logger { get; }

        private IAdvancedIntegrationTransport Transport { get; }

        private IEndpointInstanceSelectionBehavior SelectionBehavior { get; }

        private IHostStatistics Statistics { get; }

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

            _topologyMap = InitializeTopologyMap(Endpoints);

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

        private static EndpointOptions InjectTransport(EndpointOptions options, IAdvancedIntegrationTransport transport)
        {
            options.ContainerOptions ??= new DependencyContainerOptions();

            options.ContainerOptions.ManualRegistrations =
                new List<IManualRegistration>(options.ContainerOptions.ManualRegistrations)
                {
                    transport.Injection
                };

            return options;
        }

        private static IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> InitializeTopologyMap(IEnumerable<IGenericEndpoint> endpoints)
        {
            return endpoints
                .SelectMany(Messages)
                .GroupBy(pair => pair.MessageType)
                .ToDictionary(grp => grp.Key,
                    grp => grp
                        .GroupBy(innerGrp => innerGrp.Endpoint.Identity.LogicalName)
                        .ToDictionary(innerGrp => innerGrp.Key,
                            innerGrp => innerGrp.Select(pair => pair.Endpoint).ToList() as IReadOnlyCollection<IGenericEndpoint>) as IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>);

            static IEnumerable<(Type MessageType, IGenericEndpoint Endpoint)> Messages(IGenericEndpoint endpoint)
            {
                return endpoint.IntegrationTypeProvider.EndpointCommands()
                    .Concat(endpoint.IntegrationTypeProvider.EndpointQueries())
                    .Concat(endpoint.IntegrationTypeProvider.EndpointSubscriptions())
                    .Select(message => (message, endpoint));
            }
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
                        .TryAsync(() => DispatchToEndpoint(args.GeneralMessage))
                        .Invoke(ex => OnError(ex, args.GeneralMessage))
                        .ConfigureAwait(false);
                }
            }
        }

        private Task DispatchToEndpoint(IntegrationMessage message)
        {
            if (_topologyMap.TryGetValue(message.ReflectedType, out var endpoints))
            {
                var selectedEndpoints = endpoints
                    .Select(grp => SelectionBehavior.SelectInstance(message, grp.Value))
                    .ToList();

                if (selectedEndpoints.Any())
                {
                    var runningHandlers = selectedEndpoints.Select(endpoint => DispatchToEndpointInstance(message, endpoint));

                    return runningHandlers.WhenAll();
                }
            }

            throw new NotFoundException($"Target endpoint for message '{message.ReflectedType.FullName}' not found");
        }

        private Task DispatchToEndpointInstance(IntegrationMessage message, IGenericEndpoint endpoint)
        {
            return ((IExecutableEndpoint)endpoint).InvokeMessageHandler(message.DeepCopy());
        }

        private Task OnError(Exception exception, IntegrationMessage message)
        {
            Statistics.Register(exception);

            Logger.Error(exception, "Transport error on message: {0} {1}", message.ReflectedType, message.Payload);

            return Task.CompletedTask;
        }

        private void OnMessage(object? sender, IntegrationMessageEventArgs args)
        {
            _queue.Enqueue(args);
            _autoResetEvent.Set();
        }
    }
}