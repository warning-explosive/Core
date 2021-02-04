namespace SpaceEngineers.Core.GenericHost
{
    using System;
    using System.Linq;
    using Abstractions;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Enumerations;
    using GenericEndpoint.Abstractions;
    using Hosts;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Generic host extensions
    /// </summary>
    public static class GenericHostExtensions
    {
        private const string GenericEndpointAlreadyConfigured = nameof(GenericEndpointAlreadyConfigured);

        /// <summary>
        /// Use GenericEndpoint
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="endpointIdentity">EndpointIdentity</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseGenericEndpoint(
            this IHostBuilder hostBuilder,
            EndpointIdentity endpointIdentity)
        {
            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                ValidateConfiguration(ctx);

                serviceCollection.AddSingleton(BuildHostedService);

                serviceCollection.AddSingleton<IGenericEndpoint>(GetHostedService);
                serviceCollection.AddHostedService(GetHostedService);
            });

            GenericEndpointHostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                return new GenericEndpointHostedService(
                    endpointIdentity,
                    DependencyContainerPerEndpoint(endpointIdentity),
                    serviceProvider.GetRequiredService<ILogger<GenericEndpointHostedService>>());
            }

            static GenericEndpointHostedService GetHostedService(IServiceProvider serviceProvider)
            {
                return serviceProvider.GetRequiredService<GenericEndpointHostedService>();
            }
        }

        /// <summary>
        /// Use transport
        /// </summary>
        /// <param name="hostBuilder">IHostBuilder</param>
        /// <param name="transport">IIntegrationTransport</param>
        /// <param name="endpointsHosts">Endpoints hosts</param>
        /// <returns>Same IHostBuilder</returns>
        public static IHostBuilder UseTransport(
            this IHostBuilder hostBuilder,
            IIntegrationTransport transport,
            params IHost[] endpointsHosts)
        {
            return hostBuilder.ConfigureServices((ctx, serviceCollection) =>
            {
                ValidateConfiguration(ctx);

                serviceCollection.AddSingleton(_ => transport);
                serviceCollection.AddHostedService(BuildHostedService);
            });

            TransportHostedService BuildHostedService(IServiceProvider serviceProvider)
            {
                var endpoints = endpointsHosts
                    .Select(host => host.Services.GetRequiredService<IGenericEndpoint>());

                return new TransportHostedService(
                    serviceProvider.GetRequiredService<ILogger<TransportHostedService>>(),
                    serviceProvider.GetRequiredService<IIntegrationTransport>(),
                    endpoints);
            }
        }

        private static void ValidateConfiguration(HostBuilderContext ctx)
        {
            if (ctx.Properties.ContainsKey(GenericEndpointAlreadyConfigured))
            {
                throw new InvalidOperationException($"`{nameof(UseGenericEndpoint)}` or `{nameof(UseTransport)}` can only be used once on the same host instance because subsequent calls would introduce errors.");
            }

            ctx.Properties[GenericEndpointAlreadyConfigured] = null;
        }

        private static IDependencyContainer DependencyContainerPerEndpoint(EndpointIdentity endpointIdentity)
        {
            var options = new DependencyContainerOptions();

            options.OnRegistration += (s, e) =>
            {
                e.Registration.Register(() => endpointIdentity, EnLifestyle.Singleton);
                e.Registration.RegisterEmptyCollection<IEndpointInitializer>();
            };

            return DependencyContainer.Create(options); // TODO: bounded by assembly
        }
    }
}