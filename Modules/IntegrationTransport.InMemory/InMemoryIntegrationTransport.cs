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
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Abstractions;
    using GenericEndpoint.Messaging.MessageHeaders;

    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    internal class InMemoryIntegrationTransport : IIntegrationTransport,
                                                  IResolvable<IIntegrationTransport>
    {
        private readonly EndpointIdentity _endpointIdentity;

        private readonly AsyncManualResetEvent _ready;
        private readonly MessageQueue<IntegrationMessage> _inputQueue;
        private readonly DeferredQueue<IntegrationMessage> _delayedDeliveryQueue;
        private readonly IEndpointInstanceSelectionBehavior _instanceSelectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>> _topology;
        private readonly ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>> _errorMessageHandlers;

        private EnIntegrationTransportStatus _status;

        public InMemoryIntegrationTransport(
            EndpointIdentity endpointIdentity,
            IEndpointInstanceSelectionBehavior instanceSelectionBehavior)
        {
            _endpointIdentity = endpointIdentity;

            _status = EnIntegrationTransportStatus.Stopped;
            _ready = new AsyncManualResetEvent(false);

            _inputQueue = new MessageQueue<IntegrationMessage>();

            var heap = new BinaryHeap<HeapEntry<IntegrationMessage, DateTime>>(EnOrderingKind.Asc);
            _delayedDeliveryQueue = new DeferredQueue<IntegrationMessage>(heap, PrioritySelector);

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
            BindTopology(endpointIdentity, messageHandler, integrationTypeProvider.EndpointQueries(), _topology);
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

        public Task EnqueueError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            MessageReceived?.Invoke(this, new IntegrationTransportMessageReceivedEventArgs(message, exception));

            if (_errorMessageHandlers.TryGetValue(endpointIdentity, out var handlers))
            {
                return Task.WhenAll(handlers.Select(handler => handler(message, exception, token)));
            }

            throw new InvalidOperationException($"Unable to process error message. Please register error handler for {endpointIdentity} endpoint.");
        }

        public Task Accept(IntegrationMessage message, CancellationToken token)
        {
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
            return _inputQueue.Enqueue(message, token);
        }

        private Func<Exception, CancellationToken, Task> EnqueueError(IntegrationMessage message)
        {
            return (exception, token) => EnqueueError(_endpointIdentity, message, exception, token);
        }

        private Task MessageProcessingCallback(IntegrationMessage message, CancellationToken token)
        {
            return ExecutionExtensions
                .TryAsync(message, DispatchToEndpoint)
                .Catch<Exception>(EnqueueError(message))
                .Invoke(token);
        }

        private async Task DispatchToEndpoint(IntegrationMessage message, CancellationToken token)
        {
            await _ready.WaitAsync(token).ConfigureAwait(false);

            message.OverwriteHeader(new ActualDeliveryDate(DateTime.UtcNow));

            var targetEndpointLogicalName = message.GetTargetEndpoint();

            if (_topology.TryGetValue(message.ReflectedType.GenericTypeDefinitionOrSelf(), out var contravariantGroup))
            {
                var messageHandlers = contravariantGroup
                   .SelectMany(logicalGroup =>
                   {
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

                              return new Func<Task>(() =>
                              {
                                  var copy = message.ContravariantClone(reflectedType);
                                  MessageReceived?.Invoke(this, new IntegrationTransportMessageReceivedEventArgs(copy, default));
                                  return messageHandler.Invoke(copy);
                              });
                          });
                   })
                   .ToList();

                if (messageHandlers.Any())
                {
                    await messageHandlers
                       .Select(messageHandler => messageHandler.Invoke())
                       .WhenAll()
                       .ConfigureAwait(false);

                    return;
                }
            }

            throw new NotFoundException($"Target endpoint {targetEndpointLogicalName} for message '{message.ReflectedType.FullName}' not found");
        }

        private static DateTime PrioritySelector(IntegrationMessage message)
        {
            return message.ReadRequiredHeader<DeferredUntil>().Value;
        }
    }
}