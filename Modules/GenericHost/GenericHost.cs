namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using Abstractions;
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