namespace SpaceEngineers.Core.InMemoryIntegrationTransport.Defaults
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AutoRegistration.Api.Attributes;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;

    /// <inheritdoc />
    [ManuallyRegisteredComponent("We have isolation between several endpoints. Each of them have their own DependencyContainer. We need to pass the same instance of transport into all DI containers.")]
    public class DefaultEndpointInstanceSelectionBehavior : IEndpointInstanceSelectionBehavior
    {
        private static readonly ConcurrentDictionary<string, int> IndexMap
            = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public EndpointIdentity SelectInstance(
            IntegrationMessage message,
            IReadOnlyCollection<EndpointIdentity> endpoints)
        {
            if (endpoints.Count == 0)
            {
                throw new InvalidOperationException($"Process must have at least one endpoint to handle '{message.ReflectedType.FullName}'");
            }

            if (endpoints.Count == 1)
            {
                return endpoints.Single();
            }

            var indexMap = endpoints
                .Select((endpoint, i) => (endpoint, i))
                .ToDictionary(pair => pair.i, pair => pair.endpoint);
            var logicalName = endpoints.First().LogicalName;

            var index = IndexMap.GetOrAdd(logicalName, _ => 0);

            var selected = indexMap[index];

            var next = index + 1 < indexMap.Count
                ? index + 1
                : 0;

            IndexMap[logicalName] = next;

            return selected;
        }
    }
}