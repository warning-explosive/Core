namespace SpaceEngineers.Core.InMemoryIntegrationTransport
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Enumerations;
    using Basics.Exceptions;
    using Basics.Primitives;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using IntegrationTransport.Api.Abstractions;
    using IntegrationTransport.Api.Enumerations;

    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    internal class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;
        private readonly IEndpointInstanceSelectionBehavior _instanceSelectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>> _topology;

        private EnIntegrationTransportStatus _status;

        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior instanceSelectionBehavior)
        {
            _status = EnIntegrationTransportStatus.Stopped;
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingKind.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _instanceSelectionBehavior = instanceSelectionBehavior;

            _topology = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>();
        }

        public event EventHandler<IntegrationTransportStatusChangedEventArgs>? StatusChanged;

        public EnIntegrationTransportStatus Status
        {
            get => _status;

            private set
            {
                var previousValue = _status;
                _status = value;
                var eventArgs = new IntegrationTransportStatusChangedEventArgs(previousValue, value);
                StatusChanged?.Invoke(this, eventArgs);
            }
        }

        public void Bind(Type message, EndpointIdentity endpointIdentity, Func<IntegrationMessage, Task> messageHandler)
        {
            var logicalGroups = _topology.GetOrAdd(message, _ => new ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>(StringComparer.OrdinalIgnoreCase));
            var physicalGroup = logicalGroups.GetOrAdd(endpointIdentity.LogicalName, _ => new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>());
            physicalGroup.Add(endpointIdentity, messageHandler);
        }

        public async Task Enqueue(IntegrationMessage message, CancellationToken token)
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
        }

        public Task EnqueueError(IntegrationMessage message, Exception exception, CancellationToken token)
        {
            /* TODO: #112 - collect statistics */
            return Task.CompletedTask;
        }

        public Task StartBackgroundMessageProcessing(CancellationToken token)
        {
            Status = EnIntegrationTransportStatus.Starting;

            var messageProcessingTask = Task.WhenAll(
                _delayedDeliveryQueue.Run(EnqueueInput, token),
                _inputQueue.Run(MessageProcessingCallback, token));

            _ready.Set();
            Status = EnIntegrationTransportStatus.Running;

            return messageProcessingTask;
        }

        private Task EnqueueInput(IntegrationMessage message, CancellationToken token)
        {
            message.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));
            return _inputQueue.Enqueue(message, token);
        }

        private Func<Exception, CancellationToken, Task> EnqueueError(IntegrationMessage message)
        {
            return (exception, token) => EnqueueError(message, exception, token);
        }

        private Task MessageProcessingCallback(IntegrationMessage message, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(message, DispatchToEndpoint)
                .Catch<Exception>(EnqueueError(message))
                .Invoke(token);
        }

        private Task DispatchToEndpoint(IntegrationMessage message, CancellationToken token)
        {
            if (_topology.TryGetValue(message.ReflectedType, out var logicalGroups))
            {
                var messageHandlers = logicalGroups
                    .Select(logicalGroup =>
                    {
                        var selectedEndpointInstanceIdentity = SelectedEndpointInstanceIdentity(message, logicalGroup.Value);
                        return logicalGroup.Value[selectedEndpointInstanceIdentity];
                    })
                    .ToList();

                if (messageHandlers.Any())
                {
                    return messageHandlers
                        .Select(messageHandler => messageHandler.Invoke(message))
                        .WhenAll();
                }
            }

            throw new NotFoundException($"Target endpoint for message '{message.ReflectedType.FullName}' not found");
        }

        private EndpointIdentity SelectedEndpointInstanceIdentity(
            IntegrationMessage message,
            ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>> logicalGroup)
        {
            EndpointIdentity endpointIdentity;

            if (message.Payload is IIntegrationReply)
            {
                var replyTo = message.ReadRequiredHeader<ReplyTo>().Value;

                endpointIdentity = logicalGroup.Keys.Single(it => it.Equals(replyTo));
            }
            else
            {
                endpointIdentity = _instanceSelectionBehavior.SelectInstance(message, logicalGroup.Keys.ToList());
            }

            return endpointIdentity;
        }

        private static DateTime PrioritySelector(IntegrationMessage message)
        {
            return message.ReadRequiredHeader<DeferredUntil>().Value;
        }
    }
}