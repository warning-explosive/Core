namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Enumerations;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using Basics;
    using Basics.Enumerations;
    using Basics.Exceptions;
    using Basics.Primitives;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Contract.Abstractions;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;

    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    internal class InMemoryIntegrationTransport : IIntegrationTransport,
                                                  IResolvable<IIntegrationTransport>
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;
        private readonly IEndpointInstanceSelectionBehavior _instanceSelectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>>> _topology;
        private readonly ICollection<Func<IntegrationMessage, CancellationToken, Task>> _errorMessageHandlers;

        private EnIntegrationTransportStatus _status;

        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior instanceSelectionBehavior)
        {
            _status = EnIntegrationTransportStatus.Stopped;
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingKind.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _instanceSelectionBehavior = instanceSelectionBehavior;

            _topology = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>>>();
            _errorMessageHandlers = new List<Func<IntegrationMessage, CancellationToken, Task>>();
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

        public void Bind(Type message, EndpointIdentity endpointIdentity, Func<IntegrationMessage, CancellationToken, Task> messageHandler)
        {
            var logicalGroups = _topology.GetOrAdd(message, _ => new ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>>(StringComparer.OrdinalIgnoreCase));
            var physicalGroup = logicalGroups.GetOrAdd(endpointIdentity.LogicalName, _ => new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>());
            physicalGroup.Add(endpointIdentity, messageHandler);
        }

        public void BindErrorHandler(EndpointIdentity endpointIdentity, Func<IntegrationMessage, CancellationToken, Task> errorMessageHandler)
        {
            lock (_errorMessageHandlers)
            {
                _errorMessageHandlers.Add(errorMessageHandler);
            }
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
            IReadOnlyCollection<Func<IntegrationMessage, CancellationToken, Task>> errorHandlers;

            lock (_errorMessageHandlers)
            {
                errorHandlers = _errorMessageHandlers.ToList();
            }

            return errorHandlers
                .Select(handler => handler(message.Clone(), token))
                .WhenAll();
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
                Func<KeyValuePair<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>>>, bool> predicate;

                if (message.Payload is IIntegrationReply)
                {
                    var replyTo = message.ReadRequiredHeader<ReplyTo>().Value;
                    predicate = logicalGroup => logicalGroup.Key.Equals(replyTo.LogicalName, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    predicate = _ => true;
                }

                var messageHandlers = logicalGroups
                    .Where(predicate)
                    .Select(logicalGroup =>
                    {
                        var (_, instanceGroup) = logicalGroup;
                        var selectedEndpointInstanceIdentity = SelectedEndpointInstanceIdentity(message, instanceGroup);
                        return instanceGroup[selectedEndpointInstanceIdentity];
                    })
                    .ToList();

                if (messageHandlers.Any())
                {
                    return messageHandlers
                        .Select(messageHandler => messageHandler.Invoke(message, token))
                        .WhenAll();
                }
            }

            throw new NotFoundException($"Target endpoint for message '{message.ReflectedType.FullName}' not found");
        }

        private EndpointIdentity SelectedEndpointInstanceIdentity(
            IntegrationMessage message,
            ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, CancellationToken, Task>> logicalGroup)
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