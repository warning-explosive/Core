namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using Abstractions;
    using GenericEndpoint;
    using GenericEndpoint.Abstractions;
    using Internals;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Transport;

    /// <summary>
    /// Generic host entry point
    /// </summary>
    public static class GenericHost
    {
        private const string HostAlreadyConfigured = nameof(HostAlreadyConfigured);

        /// <summary>
        /// Creates InMemoryIntegrationTransport hierarchy
        /// </summary>
        /// <param name="endpointInstanceSelectionBehavior">IEndpointInstanceSelectionBehavior</param>
        /// <returns>InMemoryIntegrationTransport</returns>
        public static IIntegrationTransport InMemoryTransport(
            IEndpointInstanceSelectionBehavior endpointInstanceSelectionBehavior)
        {
            // TODO: use container & move IntegrationMessageFactory to Endpoint project
            return new InMemoryIntegrationTransport(
                endpointInstanceSelectionBehavior,
                new IntegrationMessageFactory(new[]
                {
                    new IntegratedMessageHeader()
                }));
        }

        /// <summary>
        /// Use transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="compositeEndpoint">Composite endpoint</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseTransport(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            ICompositeEndpoint compositeEndpoint)
        {
            return UseTransport(hostBuilder, transport, compositeEndpoint.Endpoints.ToArray());
        }

        /// <summary>
        /// Use transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="genericEndpoints">Generic endpoints</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseTransport(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            params IGenericEndpoint[] genericEndpoints)
        {
            ValidateEndpoints(genericEndpoints);

            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                ValidateConfiguration(ctx);

                serviceCollection.AddSingleton(_ => transport);
                serviceCollection.AddHostedService(BuildHostedService);
            });

            TransportHostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new TransportHostedService(
                    serviceProvider.GetRequiredService<ILogger<TransportHostedService>>(),
                    serviceProvider.GetRequiredService<IIntegrationTransport>(),
                    genericEndpoints);
            }
        }

        private static void ValidateEndpoints(IGenericEndpoint[] genericEndpoints)
        {
            var duplicates = genericEndpoints
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
                throw new InvalidOperationException($"`{nameof(UseTransport)}` can only be used once on the same host instance because subsequent calls would introduce errors.");
            }

            ctx.Properties[HostAlreadyConfigured] = null;
        }
    }
}