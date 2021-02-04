namespace SpaceEngineers.Core.InMemoryIntegrationTransport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GenericEndpoint.Abstractions;
    using GenericHost.Abstractions;

    internal class DefaultEndpointInstanceSelectionBehavior : IEndpointInstanceSelectionBehavior
    {
        private static readonly ConcurrentDictionary<string, int> IndexMap
            = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public IGenericEndpoint SelectInstance(
            IIntegrationMessage message,
            IReadOnlyCollection<IGenericEndpoint> endpoints)
        {
            if (endpoints.Count == 0)
            {
                throw new InvalidOperationException($"Process must have at least one endpoint to handle '{message.GetType()}'");
            }

            if (endpoints.Count == 1)
            {
                return endpoints.Single();
            }

            var indexMap = endpoints
                .Select((endpoint, i) => (endpoint, i))
                .ToDictionary(pair => pair.i, pair => pair.endpoint);

            var logicalName = endpoints.First().Identity.LogicalName;

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