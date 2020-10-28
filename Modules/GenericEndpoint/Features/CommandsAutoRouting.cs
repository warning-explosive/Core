namespace SpaceEngineers.Core.GenericEndpoint.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Basics;
    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Routing;

    /// <summary>
    /// CommandsAutoRouting via RouteToEndpointAttribute
    /// </summary>
    public class CommandsAutoRouting : Feature
    {
        /// <summary> .cctor </summary>
        public CommandsAutoRouting()
        {
            EnableByDefault();
        }

        /// <inheritdoc />
        protected override void Setup(FeatureConfigurationContext context)
        {
            var key = nameof(CommandsAutoRouting);
            var routes = ExtractCommandRoutes();
            new Action(() => context.Settings.Get<UnicastRoutingTable>().AddOrReplaceRoutes(key, routes))
               .Try()
               .Invoke(ex => context.Settings.Get<CriticalError>().Raise("Ambiguous route detected", ex));
        }

        private List<RouteTableEntry> ExtractCommandRoutes()
        {
            return AssembliesExtensions
                  .AllFromCurrentDomain()
                  .SelectMany(a => a.GetTypes())
                  .Where(t => typeof(ICommand).IsAssignableFrom(t))
                  .Select(t => new
                               {
                                   CommandType = t,
                                   RouteTo = t.GetAttribute<RouteToEndpointAttribute>()?.EndpointName
                               })
                  .Where(pair => pair.RouteTo != null)
                  .Select(pair => new RouteTableEntry(pair.CommandType, UnicastRoute.CreateFromEndpointName(pair.RouteTo)))
                  .ToList();
        }
    }
}