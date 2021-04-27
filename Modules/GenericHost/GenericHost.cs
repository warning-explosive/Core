namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using Abstractions;
    using GenericEndpoint.Executable;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generic host entry point
    /// </summary>
    public static class GenericHost
    {
        private const string HostAlreadyConfigured = nameof(HostAlreadyConfigured);

        /// <summary>
        /// Configure host
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="endpointOptions">Endpoint options</param>
        /// <returns>Configured IHostBuilder</returns>
        public static IHostBuilder ConfigureHost(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            params EndpointOptions[] endpointOptions)
        {
            ValidateEndpoints(endpointOptions);
            var advancedTransport = ValidateTransport(transport);

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                ValidateConfiguration(ctx);

                serviceCollection.AddSingleton<IAdvancedIntegrationTransport>(_ => advancedTransport);
                serviceCollection.AddSingleton<IEndpointInstanceSelectionBehavior>(_ => new EndpointInstanceSelectionBehavior());
                serviceCollection.AddSingleton<IHostStatistics>(_ => new HostStatistics());
                serviceCollection.AddHostedService(BuildHostedService);
            });

            TransportHostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new TransportHostedService(
                    serviceProvider.GetRequiredService<ILoggerFactory>(),
                    serviceProvider.GetRequiredService<IAdvancedIntegrationTransport>(),
                    serviceProvider.GetRequiredService<IEndpointInstanceSelectionBehavior>(),
                    serviceProvider.GetRequiredService<IHostStatistics>(),
                    endpointOptions);
            }
        }

        private static IAdvancedIntegrationTransport ValidateTransport(IIntegrationTransport transport)
        {
            if (transport is IAdvancedIntegrationTransport advancedTransport)
            {
                return advancedTransport;
            }

            throw new InvalidOperationException($"Integration transport should implement {nameof(IAdvancedIntegrationTransport)} abstraction");
        }

        private static void ValidateEndpoints(EndpointOptions[] endpointOptions)
        {
            var duplicates = endpointOptions
                .GroupBy(e => e.Identity)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.ToString())
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException($"Endpoint duplicates found: {string.Join(", ", duplicates)}");
            }
        }

        private static void ValidateConfiguration(HostBuilderContext ctx)
        {
            if (ctx.Properties.ContainsKey(HostAlreadyConfigured))
            {
                throw new InvalidOperationException($"`{nameof(ConfigureHost)}` can only be used once on the same host instance because subsequent calls would introduce errors.");
            }

            ctx.Properties[HostAlreadyConfigured] = null;
        }
    }
}