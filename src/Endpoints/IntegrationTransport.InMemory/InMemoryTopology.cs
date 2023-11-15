namespace SpaceEngineers.Core.IntegrationTransport.InMemory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    [Component(EnLifestyle.Singleton)]
    internal class InMemoryTopology : IInMemoryTopology,
                                      IResolvable<IInMemoryTopology>
    {
        private readonly IEndpointInstanceSelectionBehavior _instanceSelectionBehavior;

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>> _messageHandlers;
        private readonly ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>> _errorMessageHandlers;

        private readonly ConcurrentDictionary<EndpointIdentity, object?> _locker;

        public InMemoryTopology(IEndpointInstanceSelectionBehavior instanceSelectionBehavior)
        {
            _instanceSelectionBehavior = instanceSelectionBehavior;

            _messageHandlers = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>>();
            _errorMessageHandlers = new ConcurrentDictionary<EndpointIdentity, ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>>();

            _locker = new ConcurrentDictionary<EndpointIdentity, object?>();
        }

        public IEnumerable<(Func<IntegrationMessage, Task>?, Type, string?)> Dispatch(IntegrationMessage message)
        {
            if (!_messageHandlers.TryGetValue(message.ReflectedType.GenericTypeDefinitionOrSelf(), out var contravariantGroup))
            {
                return new (Func<IntegrationMessage, Task>?, Type, string?)[]
                {
                    (null, message.ReflectedType, $"Target endpoint {message.GetTargetEndpoint()} for message '{message.ReflectedType.FullName}' wasn't found")
                };
            }

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

                            (Func<IntegrationMessage, Task>?, Type, string?) tuple = !IsLocked(selectedEndpointInstanceIdentity)
                                ? (default, reflectedType, $"Topology should be locked before any active usage: {selectedEndpointInstanceIdentity}")
                                : (instanceGroup[selectedEndpointInstanceIdentity], reflectedType, default);

                            return tuple;
                        });
                });
        }

        public async Task<(bool, string?)> TryHandleError(
            EndpointIdentity endpointIdentity,
            IntegrationMessage message,
            Exception exception,
            CancellationToken token)
        {
            if (!IsLocked(endpointIdentity))
            {
                return (false, $"Topology should be locked before any active usage: {endpointIdentity}");
            }

            if (!_errorMessageHandlers.TryGetValue(endpointIdentity, out var handlers))
            {
                return (false, $"Unable to find topology for {endpointIdentity}");
            }

            await Task
                .WhenAll(handlers.Select(handler => handler(message, exception, token)))
                .ConfigureAwait(false);

            return (true, default);
        }

        public void BindMessageHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Task> messageHandler,
            IReadOnlyCollection<Type> messageTypes)
        {
            if (IsLocked(endpointIdentity))
            {
                throw new InvalidOperationException($"Topology configuration for {endpointIdentity} is locked");
            }

            foreach (var source in messageTypes)
            {
                foreach (var destination in source.IncludedTypes().Where(messageTypes.Contains))
                {
                    var contravariantGroup = _messageHandlers.GetOrAdd(source, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>>());
                    var logicalGroup = contravariantGroup.GetOrAdd(destination, _ => new ConcurrentDictionary<string, ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>>(StringComparer.OrdinalIgnoreCase));
                    var physicalGroup = logicalGroup.GetOrAdd(endpointIdentity.LogicalName, _ => new ConcurrentDictionary<EndpointIdentity, Func<IntegrationMessage, Task>>());
                    physicalGroup.Add(endpointIdentity, messageHandler);
                }
            }
        }

        public void BindErrorHandler(
            EndpointIdentity endpointIdentity,
            Func<IntegrationMessage, Exception, CancellationToken, Task> errorMessageHandler)
        {
            if (IsLocked(endpointIdentity))
            {
                throw new InvalidOperationException($"Topology configuration for {endpointIdentity} is locked");
            }

            var endpointErrorHandlers = _errorMessageHandlers.GetOrAdd(endpointIdentity, new ConcurrentBag<Func<IntegrationMessage, Exception, CancellationToken, Task>>());
            endpointErrorHandlers.Add(errorMessageHandler);
        }

        public void Lock(EndpointIdentity endpointIdentity)
        {
            _locker.Add(endpointIdentity, null);
        }

        private bool IsLocked(EndpointIdentity endpointIdentity)
        {
            return _locker.ContainsKey(endpointIdentity);
        }
    }
}