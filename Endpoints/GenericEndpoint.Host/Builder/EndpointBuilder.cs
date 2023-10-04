namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompositionRoot;
    using Contract;

    internal class EndpointBuilder : IEndpointBuilder
    {
        private readonly HashSet<string> _set;

        internal EndpointBuilder(
            EndpointIdentity endpointIdentity,
            EndpointInitializationContext context)
        {
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
            EndpointIdentity = endpointIdentity;
            Context = context;

            _set = new HashSet<string>(StringComparer.Ordinal);
        }

        public EndpointIdentity EndpointIdentity { get; }

        public EndpointInitializationContext Context { get; }

        private IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; set; }

        public IEndpointBuilder ModifyContainerOptions(Func<DependencyContainerOptions, DependencyContainerOptions> modifier)
        {
            Modifiers = Modifiers
               .Concat(new[] { modifier })
               .ToList();

            return this;
        }

        public EndpointOptions BuildOptions()
        {
            var containerOptions = new DependencyContainerOptions();

            if (Modifiers.Any())
            {
                containerOptions = Modifiers.Aggregate(containerOptions, (current, modifier) => modifier(current));
            }

            return new EndpointOptions(EndpointIdentity, containerOptions);
        }

        public void CheckMultipleCalls(string key)
        {
            var added = _set.Add(key);

            if (!added)
            {
                throw new InvalidOperationException($"Method `{key}` should be used once so as to correctly configure the endpoint instance {EndpointIdentity}");
            }
        }
    }
}