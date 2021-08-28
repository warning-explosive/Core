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
    using GenericEndpoint.Messaging;
    using IntegrationTransport.Api.Abstractions;

    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    internal class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;
        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>> _topology;

        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingKind.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _selectionBehavior = selectionBehavior;

            _topology = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>();
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

            if (message.IsDeferred())
            {
                _delayedDeliveryQueue.Enqueue(message);
            }
            else
            {
                message.SetActualDeliveryDate(DateTime.Now);
                _inputQueue.Enqueue(message);
            }
        }

        public Task EnqueueError(IntegrationMessage message, Exception exception, CancellationToken token)
        {
            /* TODO: collect statistics */
            return Task.CompletedTask;
        }

        public Task StartMessageProcessing(CancellationToken token)
        {
            var messageProcessingTask = Task.WhenAll(
                _delayedDeliveryQueue.Run(Enqueue, token),
                _inputQueue.Run(MessageProcessingCallback, token));

            _ready.Set();

            return messageProcessingTask;
        }

        private Task MessageProcessingCallback(IntegrationMessage message, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(() => DispatchToEndpoint(message))
                .Catch<Exception>()
                .Invoke(ex => EnqueueError(message, ex, token));
        }

        private Task DispatchToEndpoint(IntegrationMessage message)
        {
            if (_topology.TryGetValue(message.ReflectedType, out var logicalGroups))
            {
                var messageHandlers = logicalGroups
                    .Select(grp =>
                    {
                        var endpointIdentity = _selectionBehavior.SelectInstance(message, grp.Value.Keys.ToList());
                        return grp.Value[endpointIdentity];
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

        private static DateTime PrioritySelector(IntegrationMessage message)
        {
            return message.ReadRequiredHeader<DateTime>(IntegrationMessageHeader.DeferredUntil);
        }
    }
}