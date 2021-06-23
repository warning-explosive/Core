namespace SpaceEngineers.Core.InMemoryIntegrationTransport
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.Enumerations;
    using Basics.Exceptions;
    using Basics.Primitives;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using IntegrationTransport.Api.Abstractions;

    /// <summary>
    /// InMemoryIntegrationTransport
    /// </summary>
    [Component(EnLifestyle.Singleton, EnComponentRegistrationKind.ManuallyRegistered)]
    public class InMemoryIntegrationTransport : IIntegrationTransport
    {
        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;
        private readonly IEndpointInstanceSelectionBehavior _selectionBehavior;

        // TODO: use readonly collections
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>> _topology;

        /// <summary> .cctor </summary>
        /// <param name="selectionBehavior">IEndpointInstanceSelectionBehavior</param>
        public InMemoryIntegrationTransport(IEndpointInstanceSelectionBehavior selectionBehavior)
        {
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingKind.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

            _selectionBehavior = selectionBehavior;

            _topology = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>();
        }

        /// <summary>
        /// Configures transport topology
        /// </summary>
        /// <param name="message">Message type</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <param name="messageHandler">Message handler</param>
        public void Bind(
            Type message,
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler)
        {
            var logicalGroup = _topology.GetOrAdd(message, _ => new ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>(StringComparer.OrdinalIgnoreCase));
            var physicalGroup = logicalGroup.GetOrAdd(endpointIdentity.LogicalName, _ => new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>());
            physicalGroup.Add(endpointIdentity, messageHandler);
        }

        /// <summary>
        /// Enqueue message into input queue
        /// </summary>
        /// <param name="message">Integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing enqueue operation</returns>
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

        /// <summary>
        /// Refuse integration message
        /// </summary>
        /// <param name="failedMessage">Failed integration message</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing refuse operation</returns>
        public Task OnError(FailedMessage failedMessage, CancellationToken token)
        {
            /* TODO: collect statistics */
            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts message processing
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>Ongoing message processing operation</returns>
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
                .Invoke(ex => OnError(new FailedMessage(message, ex), token));
        }

        private Task DispatchToEndpoint(IntegrationMessage message)
        {
            if (_topology.TryGetValue(message.ReflectedType, out var logicalGroup))
            {
                var messageHandlers = logicalGroup
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