namespace SpaceEngineers.Core.GenericHost.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
    using Basics.Exceptions;
    using Basics.Primitives;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Executable;
    using GenericEndpoint.Executable.Abstractions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    internal class TransportHostedService : IHostedService, IDisposable
    {
        private readonly AsyncTaskQueue<IntegrationMessageEventArgs> _inputQueue;

        private IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> _topologyMap;
        private Task? _messageProcessingTask;
        private CancellationTokenSource? _cts;

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
            Endpoints = new Dictionary<string, IReadOnlyCollection<IGenericEndpoint>>();

            _topologyMap = new Dictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>>();
            _inputQueue = new AsyncTaskQueue<IntegrationMessageEventArgs>();
        }

        private ILogger<TransportHostedService> Logger { get; }

        private IAdvancedIntegrationTransport Transport { get; }

        private IEndpointInstanceSelectionBehavior SelectionBehavior { get; }

        private IHostStatistics Statistics { get; }

        private IReadOnlyCollection<EndpointOptions> EndpointOptions { get; }

        private IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>> Endpoints { get; set; }

        private CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public async Task StartAsync(CancellationToken token)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            Endpoints = (await EndpointOptions
                    .Select(options => Endpoint.StartAsync(InjectTransport(options, Transport), Token))
                    .WhenAll()
                    .ConfigureAwait(false))
                .GroupBy(endpoint => endpoint.Identity.LogicalName)
                .ToDictionary(grp => grp.Key,
                    grp => grp.ToList() as IReadOnlyCollection<IGenericEndpoint>,
                    StringComparer.OrdinalIgnoreCase);

            _topologyMap = InitializeTopologyMap(Endpoints);

            await Transport.Initialize(Endpoints, Token).ConfigureAwait(false);

            Logger.Information(Resources.StartedSuccessfully, Transport);
            Logger.Information(Resources.WaitingForIncomingMessages, Transport);

            Transport.OnMessage += OnMessage;
            Transport.OnError += OnError;
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
                Transport.OnError -= OnError;

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

        private static IReadOnlyDictionary<Type, IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>>> InitializeTopologyMap(
            IReadOnlyDictionary<string, IReadOnlyCollection<IGenericEndpoint>> endpoints)
        {
            return endpoints
                .SelectMany(grp => grp.Value)
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
                var args = await _inputQueue.Dequeue(Token).ConfigureAwait(false);

                if (args != null)
                {
                    await ExecutionExtensions
                        .TryAsync(() => DispatchToEndpoint(args.GeneralMessage))
                        .Invoke(ex => OnError(args.GeneralMessage, ex))
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

        private void OnMessage(object? sender, IntegrationMessageEventArgs args)
        {
            /*
             * TODO: support delayed delivery
             * 1. if (args.GeneralMessage.Headers.ContainsKey(IntegratedMessageHeader.DeferredUntil)) { _delayedDeliveryQueue.Enqueue(args); }
             * 2. register callback with delivery to input queue
             */
            _inputQueue.Enqueue(args);
        }

        private void OnError(object? sender, FailedIntegrationMessageEventArgs args)
        {
            OnError(args.GeneralMessage, args.Exception);
        }

        private Task OnError(IntegrationMessage message, Exception exception)
        {
            Statistics.RegisterFailure(message, exception);

            Logger.Error(exception, "An error occurred while processing message: {0} {1}", message.ReflectedType, message.Payload);

            return Task.CompletedTask;
        }
    }
}