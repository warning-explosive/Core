namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Basics.Enumerations;
    using Basics.Primitives;
    using CrossCuttingConcerns.Logging;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using Microsoft.Extensions.Logging;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryIntegrationTransport : IIntegrationTransport,
                                                  IConfigurableIntegrationTransport,
                                                  IExecutableIntegrationTransport,
                                                  IResolvable<IIntegrationTransport>,
                                                  IResolvable<IConfigurableIntegrationTransport>,
                                                  IResolvable<IExecutableIntegrationTransport>
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;

        private readonly ILogger _logger;
        private readonly IEndpointInstanceSelectionBehavior _instanceSelectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>> _topology;
        private readonly ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>> _errorMessageHandlers;

        private EnIntegrationTransportStatus _status;

        public InMemoryIntegrationTransport(
            ILogger logger,
            IEndpointInstanceSelectionBehavior instanceSelectionBehavior)
        {
            _status = EnIntegrationTransportStatus.Stopped;
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingDirection.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _logger = logger;
            _instanceSelectionBehavior = instanceSelectionBehavior;

            _topology = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>>();
            _errorMessageHandlers = new ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>>();
        }

        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public event EventHandler<IntegrationTransportMessageReceivedEventArgs>? MessageReceived;

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

        public void Bind(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IIntegrationTypeProvider integrationTypeProvider)
        {
            BindTopology(endpointIdentity, messageHandler, integrationTypeProvider.EndpointCommands(), _topology);
            BindTopology(endpointIdentity, messageHandler, integrationTypeProvider.EventsSubscriptions(), _topology);
            BindTopology(endpointIdentity, messageHandler, integrationTypeProvider.EndpointRequests(), _topology);
            BindTopology(endpointIdentity, messageHandler, integrationTypeProvider.RepliesSubscriptions(), _topology);

            static void BindTopology(
                EndpointIdentity endpointIdentity,
                Func<IntegrationMessage, Task> messageHandler,
                IReadOnlyCollection<Type> messageTypes,
                ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>> topology)
            {
                foreach (var source in messageTypes)
                {
                    foreach (var destination in source.IncludedTypes().Where(messageTypes.Contains))
                    {
                        var contravariantGroup = topology.GetOrAdd(source, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>());
                        var logicalGroup = contravariantGroup.GetOrAdd(destination, _ => new ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>(StringComparer.OrdinalIgnoreCase));
                        var physicalGroup = logicalGroup.GetOrAdd(endpointIdentity.LogicalName, _ => new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>());
                        physicalGroup.Add(endpointIdentity, messageHandler);
                    }
                }
            }
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            var endpointErrorHandlers = _errorMessageHandlers.GetOrAdd(endpointIdentity, new ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>());
            endpointErrorHandlers.Add(errorMessageHandler);
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

        public Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            Status = EnIntegrationTransportStatus.Starting;

            var messageProcessingTask = Task.WhenAll(
                _delayedDeliveryQueue.Run(EnqueueInput, token),
                _inputQueue.Run(HandleReceivedMessage, token));

            _ready.Set();

            Status = EnIntegrationTransportStatus.Running;

            return messageProcessingTask;
        }

        private Task EnqueueInput(IntegrationMessage message, CancellationToken token)
        {
            return _inputQueue.Enqueue(message, token);
        }

        [SuppressMessage("Analysis", "CA1031", Justification = "async event handler with void as retun value")]
        private async Task HandleReceivedMessage(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            try
            {
                ManageMessageHeaders(message);

                await Dispatch(message)
                    .Select(async pair =>
                    {
                        var (messageHandler, reflectedType) = pair;

                        if (messageHandler == null)
                        {
                            await EnqueueError(
                                    null,
                                    message,
                                    new InvalidOperationException($"Target endpoint {message.GetTargetEndpoint()} for message '{message.ReflectedType.FullName}' wasn't found"),
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
                _logger.Error(exception, $"{nameof(InMemoryIntegrationTransport)}.{nameof(HandleReceivedMessage)} - {message.ReadRequiredHeader<Id>().StringValue}");
            }
        }

        private static void ManageMessageHeaders(IntegrationMessage message)
        {
            message.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));
        }

        private IEnumerable<(Func<IntegrationMessage, Task>?, Type)> Dispatch(IntegrationMessage message)
        {
            if (_topology.TryGetValue(message.ReflectedType.GenericTypeDefinitionOrSelf(), out var contravariantGroup))
            {
                return contravariantGroup
                    .SelectMany(logicalGroup =>
                    {
                        var targetEndpointLogicalName = message.GetTargetEndpoint();
                        var reflectedType = logicalGroup.Key.ApplyGenericArguments(message.ReflectedType);

                        return logicalGroup
                            .Value
                            .Where(group => targetEndpointLogicalName.Equals("*", StringComparison.Ordinal)
                                            || group.Key.Equals(targetEndpointLogicalName, StringComparison.OrdinalIgnoreCase))
                            .Select(group =>
                            {
                                var (_, instanceGroup) = group;
                                var selectedEndpointInstanceIdentity = _instanceSelectionBehavior.SelectInstance(message, instanceGroup.Keys.ToList());
                                var messageHandler = instanceGroup[selectedEndpointInstanceIdentity];

                                (Func<IntegrationMessage, Task>?, Type) pair = (messageHandler, reflectedType);
                                return pair;
                            });
                    });
            }

            return new (Func<IntegrationMessage, Task>?, Type)[]
            {
                (null, message.ReflectedType)
            };
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

        private Task EnqueueError(
            EndpointIdentity? endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            OnMessageReceived(message, exception);

            if (endpointIdentity != null
                && _errorMessageHandlers.TryGetValue(endpointIdentity, out var handlers))
            {
                return Task.WhenAll(handlers.Select(handler => handler(message, exception, token)));
            }

            _logger.Error(exception, $"Message handling error: {message.ReflectedType.FullName}");

            return Task.CompletedTask;
        }

        private static DateTime PrioritySelector(IntegrationMessage message)
        {
            return message.ReadRequiredHeader<DeferredUntil>().Value;
        }

        private void OnMessageReceived(IntegrationMessage integrationMessage, Exception? exception)
        {
            MessageReceived?.Invoke(this, new IntegrationTransportMessageReceivedEventArgs(integrationMessage, exception));
        }
    }
}