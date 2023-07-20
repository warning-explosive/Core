namespace SpaceEngineers.Core.GenericEndpoint.Host.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompositionRoot;
    using Contract;

    internal class EndpointBuilder : IEndpointBuilder
    {
        internal EndpointBuilder(EndpointIdentity endpointIdentity)
        {
            Modifiers = Array.Empty<Func<DependencyContainerOptions, DependencyContainerOptions>>();
            EndpointIdentity = endpointIdentity;
        }

        public IReadOnlyCollection<Func<DependencyContainerOptions, DependencyContainerOptions>> Modifiers { get; protected set; }

        public EndpointIdentity EndpointIdentity { get; }

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
    }
}