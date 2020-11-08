namespace SpaceEngineers.Core.GenericEndpoint.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
    using Basics;
    using Basics.Exceptions;
    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Routing;
    using Conventions = Internals.Conventions;

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
                  .Select(cmd => new
                                 {
                                     CommandType = cmd,
                                     RouteTo = cmd.GetAttribute<RouteToEndpointAttribute>()?.EndpointName
                                            ?? throw new AttributeRequiredException(typeof(RouteToEndpointAttribute), cmd)
                                 })
                  .Select(pair => new RouteTableEntry(pair.CommandType, UnicastRoute.CreateFromEndpointName(Conventions.InputQueueName(pair.RouteTo))))
                  .ToList();
        }
    }
}